using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class FloatSlider : BaseField
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _value;
        [SerializeField] private Slider _slider;

        private FloatVariable _variable;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Bind(FloatVariable variable)
        {
            _variable = variable;
            _title.text = variable.Title;
            _slider.minValue = variable.MinValue;
            _slider.maxValue = variable.MaxValue;
            _slider.value = variable.Value;
            _slider.interactable = variable.Interactable;
            _value.text = variable.TextValue;
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
            if (!isActiveAndEnabled) return;
            _slider.SetValueWithoutNotify(variable.Value);
            _value.text = variable.TextValue;
        }

        private void OnInteractabilityChanged(bool interactable)
        {
            _slider.interactable = interactable;
        }

        public void OnSlide(float value)
        {
            _variable.Value = value;
            _value.text = _variable.TextValue;
        }
    }
}