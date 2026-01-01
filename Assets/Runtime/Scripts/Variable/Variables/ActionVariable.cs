using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartDebugger
{
    public class ActionVariable : BaseVariable
    {
        private readonly Action _action;

        public ActionVariable(string title, Action action) : base(title)
        {
            _action = action;
        }

        public void Invoke()
        {
            _action.Invoke();
        }

        public class ButtonFactory : IFieldFactory
        {
            private readonly float _width;

            public ButtonFactory(float width)
            {
                _width = width;
            }

            public GameObject Build(IVariable variable, Transform parent)
            {
                var prefab = Resources.Load<ActionButton>("Prefabs/Fields/ActionButton");
                var field = Object.Instantiate(prefab, parent);
                field.Bind((ActionVariable)variable);
                field.SetWidth(_width);
                return field.gameObject;
            }
        }
    }
}