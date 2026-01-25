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
            _dropdown.interactable = variable.Interactable;
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

        private void OnValueChanged(SerializeVariable<int> variable)
        {
            _dropdown.value = variable.Value;
        }

        private void OnInteractabilityChanged(bool interactable)
        {
            _dropdown.interactable = interactable;
        }

        public void OnSelectionChanged(int value)
        {
            _variable.Value = value;
        }
    }
}