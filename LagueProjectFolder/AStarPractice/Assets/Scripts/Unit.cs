using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    const float minPathUpdateTime = 0.2f;
    const float pathUpdateMoveThreshold = 0.5f;

    public Transform target;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;

    public float stoppingDst = 10;

    Path path;

    void Start()
    {
        StartCoroutine(UpdatePath());
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new Path(waypoints, transform.position, turnDst, stoppingDst);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    //if target position moves the path auto updates
    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < 0.3f)//if game just started then wait .3 seconds
        {
            yield return new WaitForSeconds(0.3f);
        }
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);


        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;//square because this is faster than doing the check normally
        Vector3 targetPosOld = target.position;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);//stalls so we don't check every frame of movement
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                targetPosOld = target.position;
            }
            
        }
    }


    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;

        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))//use while instead of if to make sure we can keep up with higher speeds
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (followingPath)
            {
                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)//if we are close enough to the end and have an above 0 stopping distance then start slowing
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);//closer to finish line equals smaller percent
                    if (speedPercent < 0.01f)
                    {//if we don't do this then it will take forever to reach the final bit
                        followingPath = false;
                    }
                }
                //turn
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                //move forward
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);//space.self makes it move relative to own rotation
            }

            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }
}
