using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class ActionButton : BaseField
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _text;

        private ActionVariable _variable;

        public void Bind(ActionVariable variable)
        {
            _variable = variable;
            _text.text = variable.Title;
            _button.interactable = variable.Interactable;
            variable.OnInteractabilityChanged -= OnInteractabilityChanged;
            variable.OnInteractabilityChanged += OnInteractabilityChanged;
        }

        protected override void OnDisable()
        {
            if (_variable == null) return;
            _variable.OnInteractabilityChanged -= OnInteractabilityChanged;
        }

        public void OnClick()
        {
            _variable?.Invoke();
        }

        private void OnInteractabilityChanged(bool interactable)
        {
            _button.interactable = interactable;
        }
    }
}