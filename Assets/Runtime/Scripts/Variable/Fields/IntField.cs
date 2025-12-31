using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class IntField : BaseField
    {
        [SerializeField] private Text _title;
        [SerializeField] private InputField _input;

        private IntVariable _variable;

        public void Bind(IntVariable variable)
        {
            _variable = variable;
            _title.text = variable.Title;
            _input.text = variable.TextValue;
            _variable.OnValueChanged -= OnValueChanged;
            _variable.OnValueChanged += OnValueChanged;
        }

        protected override void OnEnable()
        {
            if (_variable == null) return;
            Bind(_variable);
        }

        protected override void OnDisable()
        {
            if (_variable == null) return;
            _variable.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(SerializeVariable<int> variable)
        {
            _input.text = variable.TextValue;
        }

        public void OnEditEnd()
        {
            _variable.Value = int.TryParse(_input.text, out var intValue) ? intValue : 0;
            _input.SetTextWithoutNotify(_variable.TextValue);
        }

        public void OnPlus()
        {
            _variable.Value += 1;
        }

        public void OnMinus()
        {
            _variable.Value -= 1;
        }
    }
}