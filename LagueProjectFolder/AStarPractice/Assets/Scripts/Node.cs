using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Vector3 worldPosition;

    //keep track of position in grid
    public int gridX;
    public int gridY;

    public int movementPenalty;

    public int gCost;//dist from start node
    public int hCost;//dist from end node

    public Node parent;//allows us to assign a parent

    int heapIndex;

    public Node(bool _walkable, Vector3 _wordPos, int _gridX, int _gridY, int _penalty)
    {
        walkable = _walkable;
        worldPosition = _wordPos;

        gridX = _gridX;
        gridY = _gridY;

        movementPenalty = _penalty;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    //make work with heap interface
    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;//negative because we need to reverse what the int compare to will get us
    }
}
