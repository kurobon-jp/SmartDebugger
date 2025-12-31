namespace SmartDebugger
{
    public class FloatQueue : IndexedQueue<float>
    {
        public float Current { get; private set; }
        public float Min { get; private set; }
        public float Max { get; private set; }
        public float Average { get; private set; }

        public FloatQueue(int capacity) : base(capacity)
        {
        }

        public override void Enqueue(float current)
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

        public override void Clear()
        {
            base.Clear();
            Current = Min = Max = Average = 0f;
        }
    }
}