using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartDebugger
{
    public class SelectionVariable : IntVariable
    {
        private readonly Func<string[]> _selections;

        public string[] Selections => _selections?.Invoke();

        public override string TextValue => Selections[Value];

        public SelectionVariable(
            string title,
            Func<string[]> selections,
            int defaultValue = 0,
            string serializeKey = null,
            bool interactable = true) : base(
            title,
            defaultValue,
            serializeKey: serializeKey,
            interactable: interactable)
        {
            _selections = selections;
        }
        
        public SelectionVariable(
            string title,
            Func<string[]> selections,
            Func<int> getter,
            string serializeKey = null,
            bool interactable = true) : base(
            title,
            getter,
            serializeKey: serializeKey,
            interactable: interactable)
        {
            _selections = selections;
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
                var prefab = SDSettings.Instance.LoadPrefab<DropdownField>("DropdownField");
                var field = Object.Instantiate(prefab, parent);
                field.Bind((SelectionVariable)variable);
                field.SetWidth(_width);
                return field.gameObject;
            }
        }
    }
}