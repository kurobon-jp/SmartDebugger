namespace SmartDebugger
{
    internal class FloatQueue : IndexedQueue<float>
    {
        internal float Current { get; private set; }
        internal float Min { get; private set; }
        internal float Max { get; private set; }
        internal float Average { get; private set; }

        internal FloatQueue(int capacity) : base(capacity)
        {
        }

        internal override void Enqueue(float current)
        {
            base.Enqueue(current);
            var max = float.MinValue;
            var min = float.MaxValue;
            var sum = 0f;
            foreach (var value in Queue)
            {
                if (value > max) max = value;
                if (value < min) min = value;
                sum += value;
            }

            Current = current;
            Min = min;
            Max = max;
            Average = sum / Count;
        }

        internal override void Clear()
        {
            base.Clear();
            Current = Min = Max = Average = 0f;
        }
    }
}