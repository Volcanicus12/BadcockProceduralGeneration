using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Heap<T> where T : IHeapItem<T>
{
    T[] items;//array of type T
    int currentItemCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)//add a new item to the heap
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;

        SortUp(item);
        currentItemCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        currentItemCount--;

        //take item at end of heap and put it in slot 1
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;//returns the item that was removed
    }

    //if we find a new path, increasing something's f cost, the we do this
    public void UpdateItem(T item)
    {
        SortUp(item);//we only ever need to go up because you never go down in pathfinding
    }

    //return how many items there currently are
    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    //sees if we have an item
    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    //used when lose an item
    void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;

            int swapIndex = 0;

            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < currentItemCount)
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)//if left is lower priority than right
                    {
                        swapIndex = childIndexRight;
                    }
                }

                if(item.CompareTo(items[swapIndex]) < 0)//swa[ if a child has a higher priority than parent
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    return;//exits out of loop
                }
            }
            else//parent has no children then it is technically correct
            {
                return;
            }
        }
    }

    //used when get new item
    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)//if compare to has a higher priority then it returns 1 then we need to swap...-1 if lower priority and 0 if equal
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        //switch location in items list
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        //switch heapIndex
        int itemAIndex = itemA.HeapIndex;//temp
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

//make interface for the heap
public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}