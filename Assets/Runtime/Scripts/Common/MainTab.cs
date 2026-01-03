using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class MainTab : BaseView
    {
        [SerializeField] private Text _text;
        [SerializeField] private Image _icon;
        [SerializeField] private Toggle _toggle;
        
        private MainTabContent _prefab;
        private Transform _contentParent;
        private GameObject _content;

        public bool IsOn
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }

        public void Bind(MainTabContent prefab, Transform contentParent)
        {
            _prefab = prefab;
            _contentParent = contentParent;
            _text.text = prefab.Title;
            if (prefab.Icon != null)
            {
                _icon.gameObject.SetActive(true);
                _icon.sprite = prefab.Icon;
            }
            else
            {
                _icon.gameObject.SetActive(false);
            }
        }

        private void Show()
        {
            if (_prefab == null) return;
            _content ??= Instantiate(_prefab.gameObject, _contentParent);
            _content.SetActive(true);
        }

        private void Hide()
        {
            if (_content == null) return;
            _content.SetActive(false);
        }

        public void OnValueChanged(bool isOn)
        {
            if (isOn)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}