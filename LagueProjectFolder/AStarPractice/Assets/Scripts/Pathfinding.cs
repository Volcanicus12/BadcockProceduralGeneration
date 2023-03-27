using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    Grid grid;

    public Transform seeker, target;

    void Awake()
    {
        grid = GetComponent<Grid>();//this and Grid script must be on same game object in order to work
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))//get new path if we click jump button
        {
            FindPath(seeker.position, target.position);
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //set a timer
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

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
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbor in grid.GetNeighbors(currentNode))//get and iterate through neighbors
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))//if the node isn't walkable and not already discovered then continue
                {
                    continue;
                }
                else
                {
                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
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
                    }
                }
            }
        }
    }

    //retrace steps using parents to get path from start to end
    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;//we will be tracing path backwards

        while (currentNode != startNode){
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();//reverses reversed path to make correct

        grid.path = path;
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
