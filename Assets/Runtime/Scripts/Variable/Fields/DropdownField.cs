using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class DropdownField : BaseField
    {
        [SerializeField] private Text _title;
        [SerializeField] private Dropdown _dropdown;
        [SerializeField] private Sprite _checkMark;
        private SelectionVariable _variable;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Bind(SelectionVariable variable)
        {
            _variable = variable;
            _title.text = variable.Title;
            _dropdown.options = variable.Selections.Select(x => new Dropdown.OptionData(x, _checkMark)).ToList();
            _dropdown.value = variable.Value;
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

        private void OnValueChanged(SerializeVariable<int> variable)
        {
            _dropdown.value = variable.Value;
        }

        public void OnSelectionChanged(int value)
        {
            _variable.Value = value;
        }
    }
}