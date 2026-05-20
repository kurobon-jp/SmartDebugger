using SimpleScroll;
using UnityEngine;
using UnityEngine.UI;
using Range = SimpleScroll.Range;

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
        [SerializeField] private Scrollbar _scrollbar;
        [SerializeField] private GameObject _scrollButtons;
        [SerializeField] private LogListItem _listItem;

        [SerializeField] private ScrollRect _description;
        [SerializeField] private Text _message;
        [SerializeField] private Text _stackTrace;

        private int _selected = -1;
        private LogReceiver _logReceiver;
        private bool _isAutoScroll;
        private bool _isDirty;

        protected override void Start()
        {
            _listScroll.OnVisibleRangeChanged += OnVisibleRangeChanged;
        }

        private void OnVisibleRangeChanged(Range visibleRange)
        {
            _scrollButtons.SetActive(_scrollbar.gameObject.activeSelf);
            _isAutoScroll = visibleRange.End >= GetDataCount() - 1;
        }

        protected override void OnEnable()
        {
            _logReceiver ??= SmartDebug.Instance.LogReceiver;
            _isAutoScroll = true;
            _infoToggle.SetIsOnWithoutNotify(!_logReceiver.IsInfoFilter);
            _warnToggle.SetIsOnWithoutNotify(!_logReceiver.IsWarnFilter);
            _errorToggle.SetIsOnWithoutNotify(!_logReceiver.IsErrorFilter);
            _filterText.SetTextWithoutNotify(_logReceiver.FilterText);
            _listScroll.SetDataSource(this);
            _logReceiver.OnAdded += OnLogAdded;
            UpdateFilter();
            UpdateCount();
            ErrorIndicator.Clear();
        }

        protected override void OnDisable()
        {
            _logReceiver.OnAdded -= OnLogAdded;
            ErrorIndicator.Clear();
        }

        private void OnLogAdded(LogEntry _)
        {
            _isDirty = true;
        }

        private void UpdateCount()
        {
            var count = _logReceiver.InfoCount;
            _infoCount.text = count < 1000 ? $"{count}" : "999+";
            count = _logReceiver.WarnCount;
            _warnCount.text = count < 1000 ? $"{count}" : "999+";
            count = _logReceiver.ErrorCount;
            _errorCount.text = count < 1000 ? $"{count}" : "999+";
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

            _logReceiver.Filter(types, _filterText.text);
            _listScroll.Refresh();
        }

        private void LateUpdate()
        {
            if (_isDirty)
            {
                _isDirty = false;
                UpdateCount();
            }

            if (_isAutoScroll && !_listScroll.IsDragging && !_listScroll.IsScrolling)
            {
                
                _listScroll.Refresh(1f, false);
            }
        }

        public void Clear()
        {
            _logReceiver.Clear();
            UpdateCount();
            _description.gameObject.SetActive(false);
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
            listItem.Bind(entry, isSelected, id =>
            {
                _selected = isSelected ? -1 : id;
                _isAutoScroll = false;
                UpdateDescription();
            });
        }

        private void UpdateDescription()
        {
            if (_selected < 0)
            {
                _description.gameObject.SetActive(false);
            }
            else
            {
                _description.gameObject.SetActive(true);
                _description.normalizedPosition = Vector2.up;
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

        public void ScrollTo(float normalizedPosition)
        {
            _isAutoScroll = normalizedPosition >= 1f;
            _listScroll.Refresh(normalizedPosition, false);
        }

        public void OnCopy()
        {
            var entry = _logReceiver.FindById(_selected);
            var text = entry.Message + "\n";
            text += string.Join("\n", entry.StackTrace);
            GUIUtility.systemCopyBuffer = text;
        }
    }
}