using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class ToggleField : BaseField
    {
        [SerializeField] private Text _title;
        [SerializeField] private Toggle _toggle;

        private BoolVariable _variable;

        public void Bind(BoolVariable variable)
        {
            _variable = variable;
            _title.text = variable.Title;
            _toggle.isOn = variable.Value;
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

        private void OnValueChanged(SerializeVariable<bool> variable)
        {
            _toggle.isOn = variable.Value;
        }

        public void OnToggleChanged(bool isOn)
        {
            _variable.Value = isOn;
        }
    }
}