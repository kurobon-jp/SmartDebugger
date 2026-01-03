using System.Collections;
using UnityEngine;

namespace SmartDebugger
{
    public class ErrorIndicator : BaseView
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        private bool _isBlinking;

        public static bool HasError { get; private set; }

        protected override void Awake()
        {
            _canvas.sortingOrder = SDSettings.Instance.CanvasSortingOrder + 1;
        }

        protected override void OnEnable()
        {
            if (SDSettings.Instance.IsShowErrorIndicator)
            {
                Application.logMessageReceived += OnLogMessageReceived;
            }

            Clear();
        }

        protected override void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Clear();
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            HasError = true;
            if (type is not (LogType.Error or LogType.Exception or LogType.Assert) || _isBlinking) return;
            StopAllCoroutines();
            StartCoroutine(Blink());
        }

        private IEnumerator Blink()
        {
            var elapsed = 0f;
            var duration = 2f;
            _isBlinking = true;
            _canvasGroup.gameObject.SetActive(true);
            while (elapsed < duration)
            {
                var t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = Mathf.PingPong(t * 4, 1);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _isBlinking = false;
            _canvasGroup.gameObject.SetActive(false);
        }

        public static void Clear()
        {
            HasError = false;
        }
    }
}