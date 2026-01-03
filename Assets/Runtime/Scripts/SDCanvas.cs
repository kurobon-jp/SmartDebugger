using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(RectTransform))]
    public class SDCanvas : BaseView
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private MainTab _mainTab;
        [SerializeField] private Transform _contentParent;

        private readonly List<MainTab> _mainTabs = new();

        private FloatVariable ScaleFactor { get; } = new("ScaleFactor", 1f, 0.5f, 1.5f, "sd.scale_factor");

        protected override void Awake()
        {
            _canvasScaler.scaleFactor = ScaleFactor.Value;
            _canvas.sortingOrder = SDSettings.Instance.CanvasSortingOrder;
            var mainTabContents = SDSettings.Instance.MainTabContents;
            for (var i = 0; i < mainTabContents.Length; i++)
            {
                var prefab = mainTabContents[i];
                if (prefab is BugReportTabContent && SDSettings.Instance.BugReporter == null)
                {
                    continue;
                }

                var mainTab = Instantiate(_mainTab, _mainTab.transform.parent);
                mainTab.Bind(prefab, _contentParent);
                mainTab.IsOn = i == 0;
                _mainTabs.Add(mainTab);
            }

            _mainTab.gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Show<T>()
        {
            Show();
            var tabContents = SDSettings.Instance.MainTabContents;
            for (var i = 0; i < tabContents.Length; i++)
            {
                if (tabContents[i].GetType() != typeof(T)) continue;
                _mainTabs[i].IsOn = true;
                break;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ZoomIn()
        {
            AddScale(0.1f);
        }

        public void ZoomOut()
        {
            AddScale(-0.1f);
        }

        private void AddScale(float value)
        {
            _canvasScaler.scaleFactor = ScaleFactor.Value += value;
        }
    }
}