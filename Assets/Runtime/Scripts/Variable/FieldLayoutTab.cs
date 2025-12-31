using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class FieldLayoutTab : BaseView
    {
        [SerializeField] private Text _text;
        [SerializeField] private FieldLayoutTabContent _contentPrefab;

        private string _title;
        private Transform _contentParent;
        private IFieldLayout _fieldLayout;
        private FieldLayoutTabContent _content;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                _text.text = _title;
            }
        }

        public void Bind(IFieldLayout fieldLayout, Transform contentParent)
        {
            Title = fieldLayout.Title;
            _fieldLayout = fieldLayout;
            _contentParent = contentParent;
            gameObject.SetActive(true);
        }

        private void Show()
        {
            if (_contentPrefab == null) return;
            if (_content == null)
            {
                _content = Instantiate(_contentPrefab, _contentParent);
                _content.Bind(_fieldLayout);
            }

            _content.gameObject.SetActive(true);
        }

        private void Hide()
        {
            if (_content == null) return;
            _content.gameObject.SetActive(false);
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

        protected override void OnDestroy()
        {
            if (_content == null) return;
            Destroy(_content.gameObject);
        }
    }
}