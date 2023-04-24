using UnityEngine;
using System.Collections;
using System;

public class Heap2<T> where T : IHeapItem2<T>
{

	T[] items;
	int currentItemCount;

	public Heap2(int maxHeapSize)
	{
		items = new T[maxHeapSize];
	}

	public void Add(T item)
	{
		item.HeapIndex = currentItemCount;//tell item where it will be
		items[currentItemCount] = item; //add to end of heap
		SortUp(item);
		currentItemCount++;//we have one more item
	}

	public T RemoveFirst()
	{
		T firstItem = items[0];//grab first item
		currentItemCount--;//we have one less item
		items[0] = items[currentItemCount];//put last in first
		items[0].HeapIndex = 0;//tell item where it is
		SortDown(items[0]);//see how it needs to be sorted
		return firstItem;
	}

	public void UpdateItem(T item)
	{
		SortUp(item);//manual sort
	}

	public int Count
	{
		get
		{
			return currentItemCount;
		}
	}

	public bool Contains(T item)
	{
		return Equals(items[item.HeapIndex], item);
	}

	void SortDown(T item)
	{
		while (true)
		{
			int childIndexLeft = item.HeapIndex * 2 + 1;
			int childIndexRight = item.HeapIndex * 2 + 2;
			int swapIndex = 0;

			if (childIndexLeft < currentItemCount)//if we have one
			{
				swapIndex = childIndexLeft;

				if (childIndexRight < currentItemCount)//if we have one
				{
					if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)//-1 means that the right child is smaller...so left has lower priority
					{
						swapIndex = childIndexRight;
					}
				}

				if (item.CompareTo(items[swapIndex]) < 0)
				{
					Swap(item, items[swapIndex]);
				}
				else
				{
					return;
				}

			}
			else
			{
				return;
			}

		}
	}

	void SortUp(T item)
	{
		int parentIndex = (item.HeapIndex - 1) / 2;

		while (true)
		{
			T parentItem = items[parentIndex];
			if (item.CompareTo(parentItem) > 0)
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
		//put items into new spot in heap
		items[itemA.HeapIndex] = itemB;
		items[itemB.HeapIndex] = itemA;

		//tell items where they are now
		int itemAIndex = itemA.HeapIndex;//temp var
		itemA.HeapIndex = itemB.HeapIndex;
		itemB.HeapIndex = itemAIndex;
	}
}

public interface IHeapItem2<T> : IComparable<T>
{
	int HeapIndex
	{
		get;
		set;
	}
}