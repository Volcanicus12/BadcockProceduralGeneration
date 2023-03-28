using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();//this and Grid script must be on same game object in order to work
    }

    public void FindPath(PathRequest request, Action<PathResult> callback)//IEnumerator makes it a coroutine
    {
        //set a timer
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);

        if (startNode.walkable && targetNode.walkable)//only pathfind if both start and stop are walkable
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);//this is not hashed because it is constantly changing
            HashSet<Node> closedSet = new HashSet<Node>();//this is hashed because only thing changing is things get added

            //add start node to open set and enter a loop
            openSet.Add(startNode);

            while (openSet.Count > 0)//loop so long as set isn't empty
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)//if we made it to target
                {
                    sw.Stop();
                    print("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbor in grid.GetNeighbors(currentNode))//get and iterate through neighbors
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor))//if the node isn't walkable and not already discovered then continue
                    {
                        continue;
                    }
                    else
                    {
                        int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;
                        if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                        {//if newCost is less than neighbors current gcost or if the neighbor isn't in the openSet
                            neighbor.gCost = newMovementCostToNeighbor;
                            neighbor.hCost = GetDistance(neighbor, targetNode);

                            //set parent of neighbor to current node
                            neighbor.parent = currentNode;

                            if (!openSet.Contains(neighbor))//if our open set doesn't contain neighbor then put it there
                            {
                                openSet.Add(neighbor);
                            }
                            else
                            {
                                openSet.UpdateItem(neighbor);//update neighbors
                            }
                        }
                    }
                }
            }
        }
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Length > 0;//counteracts weird bug that happens if you move a little
        }
        callback(new PathResult(waypoints, pathSuccess, request.callback));//use callback to return to finishedProcessingPath
    }

    //retrace steps using parents to get path from start to end
    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;//we will be tracing path backwards

        while (currentNode != startNode){
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);

        Array.Reverse(waypoints);//reverses reversed path to make correct

        return waypoints;
    }

    //used to let us see the path without showing EVERY node
    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();//empty waypoint list
        Vector2 directionOld = Vector2.zero;//originally has no direction

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)//if path has changed direction
            {
                waypoints.Add(path[i].worldPosition);
            }

            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);//how far x axis do we need to move
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        else
        {
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}
