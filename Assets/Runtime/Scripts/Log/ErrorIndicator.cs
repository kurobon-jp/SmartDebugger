using System.Collections;
using UnityEngine;

namespace SmartDebugger
{
    public class ErrorIndicator : BaseView
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        private bool _isBlinking;

        protected override void Awake()
        {
            _canvas.sortingOrder = SDSettings.Instance.CanvasSortingOrder + 1;
        }

        private IEnumerator BlinkAsync()
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

        public void Blink()
        {
            if (_isBlinking) return;
            StartCoroutine(BlinkAsync());
        }
    }
}