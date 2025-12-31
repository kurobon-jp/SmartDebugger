using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class ActionButton : BaseField
    {
        [SerializeField] private Text _text;

        private ActionVariable _variable;

        public void Bind(ActionVariable variable)
        {
            _variable = variable;
            _text.text = variable.Title;
        }

        public void OnClick()
        {
            _variable?.Invoke();
        }
    }
}