using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartDebugger
{
    public class FloatVariable : SerializeVariable<float>
    {
        public float MinValue { get; }
        public float MaxValue { get; }

        public override string TextValue => $"{Value:F2}";

        public FloatVariable(string title,
            float defaultValue = 0f,
            float minValue = 0f,
            float maxValue = 1f,
            string serializeKey = null,
            bool interactable = true) : base(title, defaultValue, serializeKey, interactable)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        private protected override void SetSerializeValue(float value)
        {
            if (string.IsNullOrEmpty(SerializeKey)) return;
            PlayerPrefs.SetFloat(SerializeKey, value);
        }

        private protected override float GetSerializeValue(float defaultValue)
        {
            return string.IsNullOrEmpty(SerializeKey) ? defaultValue : PlayerPrefs.GetFloat(SerializeKey, defaultValue);
        }

        private protected override float Validate(float value)
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
                var prefab = SDSettings.Instance.LoadPrefab<FloatField>("FloatField");
                var view = Object.Instantiate(prefab, parent);
                view.SetWidth(_width);
                view.Bind((FloatVariable)variable);
                return view.gameObject;
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
                var prefab = SDSettings.Instance.LoadPrefab<FloatSlider>("FloatSlider");
                var field = Object.Instantiate(prefab, parent);
                field.SetWidth(_width);
                field.Bind((FloatVariable)variable);
                return field.gameObject;
            }
        }
    }
}