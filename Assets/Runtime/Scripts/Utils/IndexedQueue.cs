using System.Collections;
using System.Collections.Generic;

namespace SmartDebugger
{
    public class IndexedQueue<T> : IEnumerable<T>
    {
        protected List<T> Queue { get; }
        public T this[int index] => Queue[index];
        public int Count => Queue.Count;
        public int Capacity { get; }

        public IndexedQueue(int capacity = 100)
        {
            Capacity = capacity;
            Queue = new List<T>(capacity);
        }

        public virtual void Enqueue(T current)
        {
            Queue.Insert(0, current);
            if (Count > Capacity)
            {
                Queue.RemoveAt(Count - 1);
            }
        }

        public virtual void Clear()
        {
            Queue.Clear();
        }

        public IEnumerator<T> GetEnumerator() => Queue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}