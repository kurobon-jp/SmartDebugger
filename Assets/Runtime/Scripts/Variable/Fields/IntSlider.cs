using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class IntSlider : BaseField
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _value;
        [SerializeField] private Slider _slider;

        private IntVariable _variable;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Bind(IntVariable variable)
        {
            _variable = variable;
            _title.text = variable.Title;
            _slider.minValue = variable.MinValue;
            _slider.maxValue = variable.MaxValue;
            _slider.value = variable.Value;
            _value.text = variable.TextValue;
            variable.OnValueChanged -= OnValueChanged;
            variable.OnValueChanged += OnValueChanged;
        }

        protected override void OnEnable()
        {
            if (_variable == null) return;
            Bind(_variable);
        }

        protected override void OnDestroy()
        {
            _variable.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(SerializeVariable<int> variable)
        {
            if (!isActiveAndEnabled) return;
            _slider.SetValueWithoutNotify(variable.Value);
            _value.text = variable.TextValue;
        }

        public void OnSlide(float value)
        {
            _variable.Value = (int)value;
            _value.text = _variable.TextValue;
        }
    }
}