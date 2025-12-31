using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class TextField : BaseField
    {
        [SerializeField] private Text _title;
        [SerializeField] private InputField _input;

        private TextVariable _variable;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Bind(TextVariable variable)
        {
            _variable = variable;
            _title.text = variable.Title;
            _input.text = variable.TextValue;
            variable.OnValueChanged -= OnValueChanged;
            variable.OnValueChanged += OnValueChanged;
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

        private void OnValueChanged(SerializeVariable<string> variable)
        {
            if (!isActiveAndEnabled) return;
            _input.text = variable.TextValue;
        }

        public void OnEditEnd()
        {
            _variable.Value = _input.text;
            _input.SetTextWithoutNotify(_variable.TextValue);
        }
    }
}