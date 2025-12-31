using System;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    [RequireComponent(typeof(Toggle)), ExecuteAlways]
    public class ToggleColorModifer : MonoBehaviour
    {
        [Serializable]
        private struct ToggleColor
        {
            public Graphic graphic;
            public Color onColor;
            public Color offColor;

            public void SetColor(bool isOn)
            {
                if (graphic == null) return;
                graphic.color = isOn ? onColor : offColor;
            }
        }

        [SerializeField] Toggle _toggle;
        [SerializeField] ToggleColor[] _toggleColors;

        private void Awake()
        {
#if UNITY_EDITOR
            _toggle = GetComponent<Toggle>();
#endif
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
            _toggle.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(_toggle.isOn);
        }

        public void OnValueChanged(bool isOn)
        {
            if (_toggleColors == null) return;
            foreach (var toggleColor in _toggleColors)
            {
                toggleColor.SetColor(isOn);
            }
        }
    }
}