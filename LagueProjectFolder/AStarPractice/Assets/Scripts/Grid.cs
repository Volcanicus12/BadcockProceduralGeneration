using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;//how much space each node covers
    Node[,] grid;

    //start info
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Start()
    {
        //based on node radius how many nodes can fit into grid
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        //get bottom left by taking position and then subtracting the x part and the y part
        Vector3 WorldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;//transform.position is center of world

        //loop through all node collision checks
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = WorldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);//gets point

                //collision check
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));//check sphere just sees if there is a collision using position and radius
                grid[x, y] = new Node(walkable, worldPoint, x, y);//make grid
            }
        }
    }

    //returns list of nodes
    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        //find neighbors one block away
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)//should we be at 0 0 that means we are looking at current location
                {
                    continue;//pass for unity
                }
                else
                {
                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >=0 && checkY < gridSizeY)//if node is in grid
                    {
                        neighbors.Add(grid[checkX, checkY]);
                    }
                }

            }
        }

        return neighbors;
    }


    //find player location
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;//gets us a percentage to find location on node map
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;//left is 0 and right is 1...we use z for y because our y is in unity z
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);//-1 so we don't go out of array
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    public List<Node> path;

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));//gets us wire gizmo cube

        //sees if CreateGrid is working
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;//if n is walkable then set to white, but otherwise set to red

                if (path != null)
                {
                    if (path.Contains(n))
                    {
                        Gizmos.color = Color.black;
                    }
                }

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));//draws cube with a little bit of space (the 0.1)
            }
        }
    }
}
