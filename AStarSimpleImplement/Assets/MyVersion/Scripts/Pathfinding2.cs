using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding2 : MonoBehaviour
{
    PathRequestManager2 requestManager;//reference var

    Grid2 grid;

    public float zChangePenalty;//this var is meant to penalize vertical change upwards and  encourage vertical change downwards

    void Awake()
    {
        requestManager = GetComponent<PathRequestManager2>();
        grid = GetComponent<Grid2>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node2 startNode = grid.NodeFromWorldPoint(startPos);
        Node2 targetNode = grid.NodeFromWorldPoint(targetPos);

        Heap2<Node2> openSet = new Heap2<Node2>(grid.MaxSize);
        HashSet<Node2> closedSet = new HashSet<Node2>();
        openSet.Add(startNode);

        //while we have options in open set
        while(openSet.Count > 0)
        {
            Node2 currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {//if we make it
                pathSuccess = true;
                break;
            }


            //go through all neighbors
            foreach (Node2 neighbor in grid.GetNeighbors(currentNode))
            {
                //everything is walkable so don't check if walkable
                if (closedSet.Contains(neighbor))//move past if we already dealt with this
                {
                    continue;
                }

                //int newMovementCostToANeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                int newMovementCostToANeighbor = Mathf.RoundToInt(currentNode.gCost + GetDistance(currentNode, neighbor) + ((neighbor.worldPosition.y - currentNode.worldPosition.y) * zChangePenalty));//this version adds something to penalize moving upwards
                //check if shorter or neighbor not in open
                if (newMovementCostToANeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToANeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);

                    //set parent to the current node
                    neighbor.parent = currentNode;
                }

                //see if neighbor is in open set and add if not
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
            }
        }
        yield return null;//waits a frame before returning

        if (pathSuccess)//if we found a path then
        {
            waypoints = RetracePath(startNode, targetNode);
            foreach (Vector3 n in waypoints)
            {
                Debug.Log(n);
            }
        }

        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector3[] RetracePath(Node2 startNode, Node2 endNode)
    {
        List<Node2> path = new List<Node2>();

        Node2 currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        foreach (Node2 n in path)
        {
            Debug.Log(n.worldPosition);
        }
        Debug.Log("--------------------");

        Vector3[] waypoints = SimplifyPath(path);
        Debug.Log(waypoints.Length);

        //path currently in reverse so fix it
        Array.Reverse(waypoints);

        return waypoints;
    }

    //take in list of nodes and 
    Vector3[] SimplifyPath(List<Node2> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        //Vector2 directionOld = Vector2.zero;//old heading
        Vector3 directionOld = Vector3.zero;//old heading

        for (int i = 1; i < path.Count; i++)
        {
            //Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            Vector3 directionNew = path[i-1].worldPosition - path[i].worldPosition;

            if (directionNew != directionOld)//new direction and new waypoint
            {
                waypoints.Add(path[i].worldPosition);
            }

            directionOld = directionNew;
        }

        return waypoints.ToArray();//make waypoints an array and send it back
    }

    int GetDistance(Node2 nodeA, Node2 nodeB)
    {//get distances
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        //if dstx is greater then the part that will be diagonal more is the y direction
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
