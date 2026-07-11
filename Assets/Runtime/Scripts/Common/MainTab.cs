using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    internal class MainTab : BaseView
    {
        [SerializeField] private Text _text;
        [SerializeField] private Image _icon;
        [SerializeField] private Toggle _toggle;
        
        private MainTabContent _prefab;
        private Transform _contentParent;
        private GameObject _content;

        internal bool IsOn
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }
        
        internal string Text => _text.text;

        internal void Bind(MainTabContent prefab, Transform contentParent)
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

        internal void Show()
        {
            if (_prefab == null) return;
            _content ??= Instantiate(_prefab.gameObject, _contentParent);
            _content.SetActive(true);
        }

        internal void Hide()
        {
            if (_content == null) return;
            _content.SetActive(false);
        }
    }
}