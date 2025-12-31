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

        private FloatVariable ScaleFactor { get; } = new("ScaleFactor", 1f, 0.5f, 1.5f, "sd.scale_factor");

        protected override void Start()
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
                mainTab.Bind(prefab, _contentParent, i == 0);
            }

            _mainTab.gameObject.SetActive(false);
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