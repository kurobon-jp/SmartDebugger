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
            _toggle.interactable = variable.Interactable;
            variable.OnValueChanged -= OnValueChanged;
            variable.OnValueChanged += OnValueChanged;
            variable.OnInteractabilityChanged -= OnInteractabilityChanged;
            variable.OnInteractabilityChanged += OnInteractabilityChanged;
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
            _variable.OnInteractabilityChanged -= OnInteractabilityChanged;
        }

        private void OnValueChanged(SerializeVariable<bool> variable)
        {
            _toggle.isOn = variable.Value;
        }

        private void OnInteractabilityChanged(bool interactable)
        {
            _toggle.interactable = interactable;
        }

        public void OnToggleChanged(bool isOn)
        {
            _variable.Value = isOn;
        }
    }
}