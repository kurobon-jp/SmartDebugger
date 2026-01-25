using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class FloatField : BaseField
    {
        [SerializeField] private Text _title;
        [SerializeField] private InputField _input;

        private FloatVariable _variable;

        public void Bind(FloatVariable variable)
        {
            _variable = variable;
            _title.text = variable.Title;
            _input.text = variable.TextValue;
            _input.interactable = variable.Interactable;
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

        private void OnValueChanged(SerializeVariable<float> variable)
        {
            _input.text = variable.TextValue;
        }

        private void OnInteractabilityChanged(bool interactable)
        {
            _input.interactable = interactable;
        }

        public void OnEditEnd()
        {
            _variable.Value = float.TryParse(_input.text, out var value) ? value : 0;
            _input.SetTextWithoutNotify(_variable.TextValue);
        }

        public void OnPlus()
        {
            _variable.Value += _variable.MaxValue * 0.1f;
        }

        public void OnMinus()
        {
            _variable.Value -= _variable.MaxValue * 0.1f;
        }
    }
}