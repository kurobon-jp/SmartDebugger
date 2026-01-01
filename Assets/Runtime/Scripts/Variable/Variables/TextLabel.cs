using UnityEngine;

namespace SmartDebugger
{
    public class LabelVariable : IVariable
    {
        public string Title { get; }
        public bool Interactable { get; }

        public LabelVariable(string title)
        {
            Title = title;
        }

        public class Factory : IFieldFactory
        {
            private readonly float _width;

            public Factory(float width)
            {
                _width = width;
            }

            public GameObject Build(IVariable variable, Transform parent)
            {
                var prefab = Resources.Load<Label>("Prefabs/Fields/Label");
                var field = Object.Instantiate(prefab, parent);
                field.SetWidth(_width);
                field.Bind((LabelVariable)variable);
                return field.gameObject;
            }
        }
    }
}