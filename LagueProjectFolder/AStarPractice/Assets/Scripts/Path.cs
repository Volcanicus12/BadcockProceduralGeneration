using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;

    public Path(Vector3[] waypoints, Vector3 startPos, float turnDst)
    {
        lookPoints = waypoints;//lookpoint is just a rebranding of waypoint
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos);

        for (int i = 0; i < lookPoints.Length; i++)
        {
            Vector2 currentPoint = V3ToV2(lookPoints[i]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex)?currentPoint : currentPoint - dirToCurrentPoint * turnDst;//if we are on finish line then we are done, but otherwise do the math
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);//prev-dir*turn is to account for if the outcome of dir is too large and causes our line to reverse
            previousPoint = turnBoundaryPoint;

        }
    }

    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);//remember that unity z is actually our y
    }

    //helps for visualizing
    public void DrawWithGizmos()
    {
        //loops through lookPoints
        Gizmos.color = Color.black;
        foreach (Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.up, Vector2.one);
        }

        //draws turn boundaries
        Gizmos.color = Color.white;
        foreach(Line l in turnBoundaries)
        {
            l.DrawWithGizmos(10);//10 is length of line...note that this draw is using line and not the path
        }
    }
}
