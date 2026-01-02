using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public sealed class FPSGraph : GraphShaderBase
    {
        [SerializeField] private float _spikeScale = 2f;
        [SerializeField] private float _adaptiveSmooth = 0.1f;

        [SerializeField] private Text _fpsText;
        [SerializeField] private Text _avgFrameText;
        [SerializeField] private Text _maxFrameText;

        private FloatQueue _frameTimeHistory;

        private float _refreshTimer;
        private float _adaptiveMax;
        private bool _hasFixedFrameRate;
        private float _targetFrameTime;

        protected override void Awake()
        {
            _frameTimeHistory = new(_sampleCount);
            base.Awake();
            UpdateTargetFPS();
            _adaptiveMax = _targetFrameTime * _spikeScale;
        }

        private void UpdateTargetFPS()
        {
            if (Application.targetFrameRate > 0)
            {
                _targetFrameTime = 1f / Application.targetFrameRate;
                _hasFixedFrameRate = true;
            }
            else
            {
#if UNITY_2021_2_OR_NEWER
                _targetFrameTime = 1f / (float)Screen.currentResolution.refreshRateRatio.value;
#else
                _targetFrameTime = 1f / Screen.currentResolution.refreshRate;
#endif
                _hasFixedFrameRate = QualitySettings.vSyncCount > 0;
            }
        }

        private void Update()
        {
            var deltaTime = Time.unscaledDeltaTime;
            _frameTimeHistory.Enqueue(deltaTime);
            _refreshTimer += deltaTime;
            if (_refreshTimer >= 1f)
            {
                _fpsText.text = $"{1f / _frameTimeHistory.Average:F2}";
                _avgFrameText.text = $"{_frameTimeHistory.Average * 1000f:F2} ms";
                _maxFrameText.text = $"{_frameTimeHistory.Max * 1000f:F2} ms";
                _refreshTimer = 0f;
            }

            var desiredMax = Mathf.Max(deltaTime, _targetFrameTime * _spikeScale);
            _adaptiveMax = Mathf.Lerp(_adaptiveMax, desiredMax, _adaptiveSmooth);
            var fpsNorm = Mathf.Clamp01(deltaTime / _adaptiveMax);
            var warnNorm = Mathf.InverseLerp(_targetFrameTime, _targetFrameTime * 2f, deltaTime);
            var targetNorm = _hasFixedFrameRate ? Mathf.Clamp01(_targetFrameTime / _adaptiveMax) : 0f;
            PushSample(new Vector4(fpsNorm, warnNorm, targetNorm, 0));
        }
    }
}