using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartDebugger
{
    public class IntVariable : SerializeVariable<int>
    {
        public int MinValue { get; }
        public int MaxValue { get; }

        public IntVariable(string title,
            int defaultValue = 0,
            int minValue = 0,
            int maxValue = int.MaxValue,
            string serializeKey = null) : base(title, defaultValue, serializeKey)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        private protected override void SetSerializeValue(int value)
        {
            if (string.IsNullOrEmpty(SerializeKey)) return;
            PlayerPrefs.SetInt(SerializeKey, value);
        }

        private protected override int GetSerializeValue(int defaultValue)
        {
            return string.IsNullOrEmpty(SerializeKey) ? defaultValue : PlayerPrefs.GetInt(SerializeKey, defaultValue);
        }

        private protected override int Validate(int value)
        {
            return Mathf.Clamp(value, MinValue, MaxValue);
        }

        public class FieldFactory : IFieldFactory
        {
            private readonly float _width;

            public FieldFactory(float width)
            {
                _width = width;
            }

            public GameObject Build(IVariable variable, Transform parent)
            {
                var prefab = Resources.Load<IntField>("Prefabs/Fields/IntField");
                var field = Object.Instantiate(prefab, parent);
                field.SetWidth(_width);
                field.Bind((IntVariable)variable);
                return field.gameObject;
            }
        }

        public class SliderFactory : IFieldFactory
        {
            private readonly float _width;

            public SliderFactory(float width)
            {
                _width = width;
            }

            public GameObject Build(IVariable variable, Transform parent)
            {
                var prefab = Resources.Load<IntSlider>("Prefabs/Fields/IntSlider");
                var field = Object.Instantiate(prefab, parent);
                field.SetWidth(_width);
                field.Bind((IntVariable)variable);
                return field.gameObject;
            }
        }
    }
}