using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    internal class ErrorIndicator : BaseView
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _icon;
        [SerializeField] private Text _text;
        [SerializeField] private GameObject _background;

        private bool _isBlinking;

        protected override void Awake()
        {
            _canvas.sortingOrder = SDSettings.Instance.CanvasSortingOrder + 1;
        }

        private IEnumerator BlinkAsync()
        {
            const float duration = 2f;
            _isBlinking = true;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = Mathf.Clamp01(elapsed / duration);
                _icon.alpha = 1f - Mathf.PingPong(t * 6, 1);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _icon.alpha = 1f;
            _isBlinking = false;
        }

        internal void Blink(string text)
        {
            if (_isBlinking) return;
            _text.text = text;
            _background.SetActive(true);
            StartCoroutine(BlinkAsync());
        }

        internal void Clear()
        {
            StopAllCoroutines();
            _background.SetActive(false);
            _isBlinking = false;
            _text.text = "";
            _icon.alpha = 0f;
        }
    }
}