using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid2 : MonoBehaviour
{
    public bool displayGridGizmos;

    public Vector2 gridWorldSize;//how big is grid
    public float nodeRadius;//how big each node will be
    Node2[,] grid;
    private GameObject character;
    public int enlargenGrid;

    float nodeDiameter;
    int gridSizeX;
    int gridSizeY;

    public float maxMapHeight;

    public float maxJump;
    public float maxSlope;

    CapsuleCollider charCapsule;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;

        character = GameObject.Find("RigidBodyFPSController");
        charCapsule = character.GetComponent<CapsuleCollider>();
    }

    //get max size of grid
    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    public void CreateGrid()
    {
        //calculate grid x and y(z)
        float x = Mathf.Abs(transform.position.x - character.transform.position.x) + enlargenGrid;//size of x needs to be the difference plus some space
        float z = Mathf.Abs(transform.position.z - character.transform.position.z) + enlargenGrid;//same goes for our z
        gridWorldSize = new Vector2(x, z);//z is the "y" if we look top down

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);//how many nodes in x
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);//same for y

        //start making grid (this is where Lague starts this method)
        grid = new Node2[gridSizeX, gridSizeY];
        Vector3 middle = (transform.position + character.transform.position) / 2;
        Vector3 bottomLeft = middle - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;//get bottom left corner

        //everything is walkable, but this is a good way to get all of the height data
        for (int xInt = 0; xInt < gridSizeX; xInt++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 rayStartLocation = bottomLeft + Vector3.right * (xInt * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius) + Vector3.up * maxMapHeight;

                //ray down
                Ray ray = new Ray(rayStartLocation, Vector3.down);//ray goes from enemy to character
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, maxMapHeight + 10))//go a little farther down just to ensure we are good
                {//if get a hit
                    //Debug.Log(hit.point);
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Character"))//if we hitting character then move it down the player size
                    {
                        rayStartLocation = hit.point;
                        Vector3 hitWithoutCharHeight = rayStartLocation - Vector3.up * character.transform.localScale.y;//y height dif
                        //Debug.Log(hitWithoutCharHeight);
                        grid[xInt, y] = new Node2(hitWithoutCharHeight, xInt, y);
                    }
                    else
                    {
                        grid[xInt, y] = new Node2(hit.point, xInt, y);
                    }
                }
            }
        }
    }

    //find neighbor nodes
    public List<Node2> GetNeighbors(Node2 node)
    {
        List<Node2> neighbors = new List<Node2>();
        for (int x = -1; x <= 1; x++)//check nodes around
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0)
                {
                    continue;//skips this iteration
                }

                //else...see if we are inside grid
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >=0 && checkY < gridSizeY)
                {//if we are in grid
                    float heightDif = 0;//set to 0 then calculate
                    try
                    {
                        heightDif = grid[checkX, checkY].worldPosition.y - grid[node.gridX, node.gridY].worldPosition.y;
                    }
                    catch
                    {//I don't know why but sometimes this throws an error...if it does then we will just do a continue
                        heightDif = maxJump + 1;
                    }
                    if (heightDif > maxJump || heightDif < -maxJump)//if we can't jump that high or fall that farw then skip
                    {
                        //Debug.Log($"too big of a jump {heightDif}");
                        continue;
                    }

                    //we do / z+x because one of the two will be 0
                    float adj = Mathf.Sqrt(Mathf.Pow(grid[checkX, checkY].worldPosition.z - grid[node.gridX, node.gridY].worldPosition.z,2) + Mathf.Pow(grid[checkX, checkY].worldPosition.x - grid[node.gridX, node.gridY].worldPosition.x,2));//we only need calc adjacent side of triangle since the height dif is calculated prior
                    float slopeRad = Mathf.Atan(heightDif/adj);//get theta in radians
                    float slopeDeg = slopeRad * 180/Mathf.PI;//convert radians to degrees
                    if (slopeDeg > maxSlope || slopeDeg < -maxSlope)//if slope is too steep then don't do it
                    {
                        //Debug.Log($"too steep {slopeDeg}");
                        continue;
                    }

                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    //returns where character is
    public Node2 NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 worldPosWithAdjustmentToGrid = worldPosition - ((transform.position + character.transform.position) / 2);//get position in relation to the grid (this makes it as if we are at 000)
        //find x and y for grid
        float percentX = Mathf.Clamp01((worldPosWithAdjustmentToGrid.x + gridWorldSize.x / 2) / gridWorldSize.x);//need to do position - distance from center of grid
        float percentY = Mathf.Clamp01((worldPosWithAdjustmentToGrid.z + gridWorldSize.y / 2) / gridWorldSize.y);

        int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    //see if we are next to the playerNode
    public bool TouchingPlayer()
    {
        //get grid positions of player and enemy
        Node2 playerNode = null;//playerNode needs to be set to blank first (to appease the errors)
        try//if playerNode is no longer within the bounds then we definitely aren't next to it.
        {
            playerNode = NodeFromWorldPoint(character.transform.position);//need to try this
        }
        catch
        {
            return false;
        }
        Node2 enemyNode = NodeFromWorldPoint(this.transform.position);

        //if x and y are each off by 1 or less then we are adjacent
        if (Mathf.Abs(playerNode.gridX - enemyNode.gridX) <= 1 && Mathf.Abs(playerNode.gridY - enemyNode.gridY) <= 1)
        {
            if (((character.transform.position.y - (charCapsule.height/2)) - (this.transform.position.y + this.transform.localScale.y/2)) <= 0.5)//we need to compare the bottom of char and top of enemy since enemy can't jump
            {//check if we are over them and not touching them
                return true;
            }
        }
        return false;

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube((transform.position + character.transform.position) / 2, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));//center is between character and enemy



        if (grid != null && displayGridGizmos)
        {
            //Node2 playerNode = NodeFromWorldPoint(character.transform.position);
            //Node2 enemyNode = NodeFromWorldPoint(this.transform.position);

            Gizmos.color = Color.white;
            foreach (Node2 n in grid)
            {
                Gizmos.color = Color.white;

                if (NodeFromWorldPoint(character.transform.position) == n)
                {
                    Gizmos.color = Color.blue;
                }
                if(NodeFromWorldPoint(this.transform.position) == n)
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                //Debug.DrawRay(n.worldPosition + (Vector3.up * maxMapHeight), Vector3.down * (maxMapHeight + 10));
            }
        }
    }
}
