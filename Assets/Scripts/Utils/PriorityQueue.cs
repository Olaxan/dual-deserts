using System;
using System.Collections;
using System.Collections.Generic;

class PriorityQueue<TItem, TPriority> where TPriority : IComparable
{
    private SortedList<TPriority, Queue<TItem>> pq = new SortedList<TPriority, Queue<TItem>>();
    public int Count { get; private set; }

    public void Enqueue(TItem item, TPriority priority)
    {
        ++Count;
        if (!pq.ContainsKey(priority)) pq[priority] = new Queue<TItem>();
        pq[priority].Enqueue(item);
    }

    public TItem Dequeue()
    {
        --Count;
        var queue = pq.Values[0];
        if (queue.Count == 1) pq.RemoveAt(0);
        return queue.Dequeue();
    }
}

class PriorityQueue<TItem> : PriorityQueue<TItem, int> { }