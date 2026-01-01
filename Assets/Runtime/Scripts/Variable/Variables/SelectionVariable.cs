using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartDebugger
{
    public class SelectionVariable : IntVariable
    {
        private readonly Func<string[]> _getter;

        public string[] Selections => _getter?.Invoke();

        public override string TextValue => Selections[Value];

        public SelectionVariable(
            string title,
            Func<string[]> getter,
            int defaultValue = 0,
            string serializeKey = null) : base(title, defaultValue, serializeKey: serializeKey)
        {
            _getter = getter;
        }

        private protected override int Validate(int value)
        {
            var selections = Selections;
            return Mathf.Clamp(value, 0, selections.Length - 1);
        }

        public class DropdownFactory : IFieldFactory
        {
            private readonly float _width;

            public DropdownFactory(float width)
            {
                _width = width;
            }

            public GameObject Build(IVariable variable, Transform parent)
            {
                var prefab = Resources.Load<DropdownField>("Prefabs/Fields/DropdownField");
                var field = Object.Instantiate(prefab, parent);
                field.Bind((SelectionVariable)variable);
                field.SetWidth(_width);
                return field.gameObject;
            }
        }
    }
}