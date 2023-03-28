using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;//how much space each node covers
    Node[,] grid;

    public TerrainType[] walkableRegions;
    LayerMask walkableMask;//layer mask to hold all walkable layers
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    public int obstacleProximityPenalty = 10;

    //start info
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    void Awake()//this is awake instead of start because these need to happen before anything has the chance to be called
    {
        //based on node radius how many nodes can fit into grid
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegions)
        {
            walkableMask.value = walkableMask | region.terrainMask.value;//| is a bitwise or
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);//add layer and penalty to dict
        }

        CreateGrid();
    }

    public int MaxSize
    {
        get{
            return gridSizeX * gridSizeY;
        }
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

                int movementPenalty = 0;

                //raycast for movePenalty
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, walkableMask))//if get a hit
                {
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);//gets movement penalty
                }

                if (!walkable)
                {
                    movementPenalty += obstacleProximityPenalty;
                }

                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);//make grid
            }
        }

        BlurPenaltyMap(3);
    }

    //box blur
    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;//+1 because we need an odd size so we have a central square
        int kernelExtents = (kernelSize - 1) / 2;

        //temporary grids
        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        //go through row by row for horizontal pass
        for (int y = 0; y < gridSizeY; y++)
        {
            //for first column...can't do the -+ trick until I do the first row
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);//get index
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;//add grid point to horizontal pass
            }

            //rest of columns
            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0 , gridSizeX);//grab value of what we are leaving behind
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX-1);//grab what we are picking up

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;//put our value - what we lost + what we gained into the horizGrid
            }
        }

        //go through row by row for vertical pass
        for (int x = 0; x < gridSizeX; x++)
        {
            //for first column...can't do the -+ trick until I do the first row
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);//get index
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];//add grid point to vert pass stemming from horiz
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));//take our number and average between kernel squares
            grid[x, 0].movementPenalty = blurredPenalty;//set new values


            //rest of columns
            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);//grab value of what we are leaving behind
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);//grab what we are picking up

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex]+ penaltiesHorizontalPass[x, addIndex];//put our value - what we lost + what we gained into the horizGrid

                //get final blurred penalty for each node
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));//take our number and average between kernel squares
                grid[x, y].movementPenalty = blurredPenalty;//set new values

                if (blurredPenalty > penaltyMax)
                {
                    penaltyMax = blurredPenalty;
                }
                if (blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;
                }
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


    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));//gets us wire gizmo cube
        //sees if CreateGrid is working
        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));

                Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;//if n is walkable then set to white, but otherwise set to red
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));//draws cube with a little bit of space (the 0.1)
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
