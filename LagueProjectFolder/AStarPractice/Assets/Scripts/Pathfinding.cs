using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        FindPath(seeker.position, target.position);
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();//this is not hashed because it is constantly changing
        HashSet<Node> closedSet = new HashSet<Node>();//this is hashed because only thing changing is things get added

        //add start node to open set and enter a loop
        openSet.Add(startNode);

        while (openSet.Count > 0)//loop so long as set isn't empty
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost )//if fCost is less than current smallest fCost...in case of tie refer to hCost
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);//remove from open set and add to closed
            closedSet.Add(currentNode);

            if (currentNode == targetNode)//if we made it to target
            {
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
