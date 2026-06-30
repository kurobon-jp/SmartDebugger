using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(RectTransform))]
    internal class SDCanvas : BaseView
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private List<CanvasScaler> _canvasScalers;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private MainTab _mainTab;
        [SerializeField] private Transform _contentParent;

        private bool _wasCursorVisible;
        private CursorLockMode _cursorLockMode;

        private readonly List<MainTab> _mainTabs = new();
        private readonly FloatVariable _scaleFactor = new("ScaleFactor", 1f, 0.5f, 1.5f, "sd.scale_factor");
        private readonly IntVariable _currentTab = new("CurrentTab", serializeKey: "sd.current_tab");

        protected override void Awake()
        {
            _canvasScalers.ForEach(x => x.scaleFactor = _scaleFactor.Value);
            _canvas.sortingOrder = SDSettings.Instance.CanvasSortingOrder;
            var mainTabContents = SDSettings.Instance.MainTabContents;
            var currentTab = Mathf.Clamp(_currentTab.Value, 0, mainTabContents.Length - 1);
            for (var i = 0; i < mainTabContents.Length; i++)
            {
                var prefab = mainTabContents[i];
                if (prefab is BugReportTabContent && SDSettings.Instance.BugReporter == null)
                {
                    continue;
                }

                var mainTab = Instantiate(_mainTab, _mainTab.transform.parent);
                mainTab.Bind(prefab, _contentParent);
                mainTab.IsOn = i == currentTab;
                _mainTabs.Add(mainTab);
            }

            _mainTab.gameObject.SetActive(false);
        }

        internal void Open()
        {
            SetAlpha(1f);
            gameObject.SetActive(true);
            _wasCursorVisible = Cursor.visible;
            _cursorLockMode = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        internal void Open<T>()
        {
            Open();
            var tabContents = SDSettings.Instance.MainTabContents;
            for (var i = 0; i < tabContents.Length; i++)
            {
                if (tabContents[i].GetType() != typeof(T)) continue;
                _mainTabs[i].IsOn = true;
                break;
            }
        }

        internal void Close()
        {
            gameObject.SetActive(false);
            Cursor.visible = _wasCursorVisible;
            Cursor.lockState = _cursorLockMode;
        }

        internal void SetAlpha(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }

        internal void ZoomIn()
        {
            AddScale(0.1f);
        }

        internal void ZoomOut()
        {
            AddScale(-0.1f);
        }

        private void AddScale(float value)
        {
            _scaleFactor.Value += value;
            _canvasScalers.ForEach(x => x.scaleFactor = _scaleFactor.Value);
        }

        internal void OnValueChanged(MainTab tab)
        {
            if (tab.IsOn)
            {
                tab.Show();
                _currentTab.Value = _mainTabs.IndexOf(tab);
            }
            else
            {
                tab.Hide();
            }
        }
    }
}