using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node2 : IHeapItem2<Node2>
{
    public Vector3 worldPosition;//where is our node in the world
    public int gridX;//grid pos
    public int gridY;

    public int gCost;//to node
    public int hCost;//distance to end node

    //parent node
    public Node2 parent;

    int heapIndex;

    public Node2(Vector3 _worldPos, int _gridX, int _gridY)
    {
        worldPosition = _worldPos;
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


    //return 1 if current item has higher priority...so if integer is lower
    public int CompareTo(Node2 nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);

        if(compare == 0)//hCost decides a tie
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;//integer.CompareTo gets us 1 if current is higher so we gotta flip it because we want what is lower
    }
}
