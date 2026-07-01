using System.Collections;
using System.Collections.Generic;

namespace SmartDebugger
{
    internal class IndexedQueue<T> : IEnumerable<T>
    {
        protected List<T> Queue { get; }
        internal T this[int index] => Queue[index];
        internal int Count => Queue.Count;
        internal int Capacity { get; }

        internal IndexedQueue(int capacity = 100)
        {
            Capacity = capacity;
            Queue = new List<T>(capacity);
        }

        internal virtual void Enqueue(T current)
        {
            Queue.Insert(0, current);
            if (Count > Capacity)
            {
                Queue.RemoveAt(Count - 1);
            }
        }

        internal virtual void Clear()
        {
            Queue.Clear();
        }

        public IEnumerator<T> GetEnumerator() => Queue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}