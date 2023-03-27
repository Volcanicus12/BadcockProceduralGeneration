using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;

    //keep track of position in grid
    public int gridX;
    public int gridY;

    public int gCost;//dist from start node
    public int hCost;//dist from end node

    public Node parent;//allows us to assign a parent

    public Node(bool _walkable, Vector3 _wordPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _wordPos;

        gridX = _gridX;
        gridY = _gridY;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}
