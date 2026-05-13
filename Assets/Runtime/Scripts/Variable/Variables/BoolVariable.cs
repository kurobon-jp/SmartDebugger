using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartDebugger
{
    public class BoolVariable : SerializeVariable<bool>
    {
        public BoolVariable(
            string title,
            bool defaultValue = false,
            string serializeKey = null,
            bool interactable = true) : base(title,
            defaultValue, serializeKey, interactable)
        {
        }

        public BoolVariable(string title,
            Func<bool> getter,
            string serializeKey = null,
            bool interactable = true) : base(title, getter, serializeKey, interactable)
        {
        }

        private protected override void SetSerializeValue(bool value)
        {
            if (string.IsNullOrEmpty(SerializeKey)) return;
            PlayerPrefs.SetInt(SerializeKey, Value ? 1 : 0);
        }

        private protected override bool GetSerializeValue(bool defaultValue)
        {
            return string.IsNullOrEmpty(SerializeKey)
                ? defaultValue
                : PlayerPrefs.GetInt(SerializeKey, defaultValue ? 1 : 0) == 1;
        }

        public class ToggleFactory : IFieldFactory
        {
            private readonly float _width;

            public ToggleFactory(float width)
            {
                _width = width;
            }

            public GameObject Build(IVariable variable, Transform parent)
            {
                var prefab = SDSettings.Instance.LoadPrefab<ToggleField>("ToggleField");
                var field = Object.Instantiate(prefab, parent);
                field.SetWidth(_width);
                field.Bind((BoolVariable)variable);
                return field.gameObject;
            }
        }
    }
}