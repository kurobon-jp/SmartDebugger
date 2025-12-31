using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class LogListItem : BaseView
    {
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _selected;
        [SerializeField] private Text _message;
        [SerializeField] private Text _stackTrace;

        private int _id;
        private Action<int> _onSelected;

        private static readonly StringBuilder StringBuilder = new();

        public void Bind(in LogEntry entry, bool selected, Action<int> onSelected = null)
        {
            _id = entry.Id;
            _onSelected = onSelected;
            _selected.gameObject.SetActive(selected);
            _icon.sprite = _sprites[entry.Types.GetIndex()];
            _message.text = entry.Message;
            _stackTrace.text = entry.StackTrace[0];
        }

        public void OnClick()
        {
            _onSelected?.Invoke(_id);
        }
    }
}