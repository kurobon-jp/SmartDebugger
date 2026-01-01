using UnityEngine;

namespace SmartDebugger
{
    public class TextVariable : SerializeVariable<string>
    {
        public TextVariable(string title, string serializeKey = null, string defaultValue = "")
            : base(title, defaultValue, serializeKey)
        {
        }

        private protected override void SetSerializeValue(string value)
        {
            if (string.IsNullOrEmpty(SerializeKey)) return;
            PlayerPrefs.SetString(SerializeKey, value);
        }

        private protected override string GetSerializeValue(string defaultValue)
        {
            return string.IsNullOrEmpty(SerializeKey)
                ? defaultValue
                : PlayerPrefs.GetString(SerializeKey, defaultValue);
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
                var prefab = Resources.Load<TextField>("Prefabs/Fields/TextField");
                var field = Object.Instantiate(prefab, parent);
                field.SetWidth(_width);
                field.Bind((TextVariable)variable);
                return field.gameObject;
            }
        }
    }
}