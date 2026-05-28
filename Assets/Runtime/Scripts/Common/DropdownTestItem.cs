using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class DropdownTestItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
    {
        [SerializeField] private Text _text;
        [SerializeField] private Toggle _toggle;

        private int _value;
        private Action<int> _onClick;

        private void Start()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool isOn)
        {
            _onClick.Invoke(_value);
        }

        public void Setup(int value, string text, bool selected, Action<int> onClick)
        {
            _value = value;
            _onClick = onClick;
            _text.text = text;
            _toggle.toggleTransition = Toggle.ToggleTransition.None;
            _toggle.SetIsOnWithoutNotify(selected);
            _toggle.toggleTransition = Toggle.ToggleTransition.Fade;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            _onClick?.Invoke(-1);
        }
    }
}