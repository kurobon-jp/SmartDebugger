using SimpleScroll;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class LogTabContent : MainTabContent, IDataSource
    {
        [SerializeField] private Text _infoCount;
        [SerializeField] private Text _warnCount;
        [SerializeField] private Text _errorCount;

        [SerializeField] private Toggle _infoToggle;
        [SerializeField] private Toggle _warnToggle;
        [SerializeField] private Toggle _errorToggle;
        [SerializeField] private InputField _filterText;

        [SerializeField] private FixedListScroll _listScroll;
        [SerializeField] private LogListItem _listItem;

        [SerializeField] private GameObject _description;
        [SerializeField] private Text _message;
        [SerializeField] private Text _stackTrace;

        private int _selected = -1;
        private float _normalizedPosition;
        private float _scrollPosition;
        private LogReceiver _logReceiver;

        protected override void OnEnable()
        {
            base.OnEnable();
            _logReceiver ??= SmartDebug.Instance.LogReceiver;
            _infoToggle.SetIsOnWithoutNotify(!_logReceiver.IsInfoFilter);
            _warnToggle.SetIsOnWithoutNotify(!_logReceiver.IsWarnFilter);
            _errorToggle.SetIsOnWithoutNotify(!_logReceiver.IsErrorFilter);
            _filterText.SetTextWithoutNotify(_logReceiver.FilterText);
            _listScroll.SetDataSource(this);
            _logReceiver.OnAdding += OnLogAdding;
            _logReceiver.OnAdded += OnLogAdded;
            UpdateFilter();
            UpdateCount();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _logReceiver.OnAdding -= OnLogAdding;
            _logReceiver.OnAdded -= OnLogAdded;
        }

        private bool _isRequestBottomScroll;

        private void OnLogAdding()
        {
            _isRequestBottomScroll |= _listScroll.VisibleRange.End >= GetDataCount() - 1;
        }

        private void OnLogAdded()
        {
            UpdateCount();
        }

        private void UpdateCount()
        {
            var count = _logReceiver.InfoCount;
            _infoCount.text = count < 1000 ? $"{count}" : "+999";
            count = _logReceiver.WarnCount;
            _warnCount.text = count < 1000 ? $"{count}" : "+999";
            count = _logReceiver.ErrorCount;
            _errorCount.text = count < 1000 ? $"{count}" : "+999";
        }

        public void UpdateFilter()
        {
            var types = LogTypes.None;
            if (!_infoToggle.isOn)
            {
                types |= LogTypes.Info;
            }

            if (!_warnToggle.isOn)
            {
                types |= LogTypes.Warning;
            }

            if (!_errorToggle.isOn)
            {
                types |= LogTypes.Error;
            }

            var normalizedPos = _listScroll.NormalizedPosition;
            if (normalizedPos > 0)
            {
                _normalizedPosition = normalizedPos;
            }

            _logReceiver.Filter(types, _filterText.text);
            _listScroll.Refresh(_normalizedPosition);
        }

        private void Update()
        {
            if (!_isRequestBottomScroll) return;
            _isRequestBottomScroll = false;
            if (_listScroll.IsDragging) return;
            _listScroll.Refresh(1f);
        }

        public void Clear()
        {
            _logReceiver.Clear();
            UpdateCount();
            _description.SetActive(false);
            _selected = -1;
            _listScroll.Refresh();
        }

        public void ClearFilterText()
        {
            _filterText.text = string.Empty;
            UpdateFilter();
        }

        public int GetDataCount()
        {
            return _logReceiver.Count;
        }

        public void SetData(int index, GameObject go)
        {
            if (!go.TryGetComponent(out LogListItem listItem)) return;
            var entry = _logReceiver.FindByIndex(index);
            var isSelected = _selected == entry.Id;
            listItem.Bind(entry, isSelected, i =>
            {
                if (isSelected)
                {
                    _selected = -1;
                }
                else
                {
                    _selected = entry.Id;
                }

                UpdateDescription();
            });
        }

        private void UpdateDescription()
        {
            if (_selected < 0)
            {
                _description.SetActive(false);
            }
            else
            {
                _description.SetActive(true);
                var entry = _logReceiver.FindById(_selected);
                _message.text = entry.Message;
                _stackTrace.text = string.Join("\n", entry.StackTrace);
            }

            _listScroll.Refresh();
        }

        public GameObject GetCellView(int index)
        {
            return _listItem.gameObject;
        }
    }
}