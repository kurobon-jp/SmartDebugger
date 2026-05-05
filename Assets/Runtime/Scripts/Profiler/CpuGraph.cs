using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    internal sealed class CpuGraph : GraphShaderBase
    {
        private static readonly int MaxTimeID = Shader.PropertyToID("_MaxTime");
        private static readonly int TargetTimeID = Shader.PropertyToID("_TargetTime");

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Text _fpsText;
        [SerializeField] private Text _avgFrameText;
        [SerializeField] private Text _maxFrameText;
        [SerializeField] private TargetLine _targetLine;

        private float _targetFrameRate = 60f;
        private float _elapsed;
        private Rect _rect;

        private void Start()
        {
            UpdateTargetFPS();
        }

        private void UpdateTargetFPS()
        {
            if (Application.targetFrameRate > 0)
            {
                _targetFrameRate = Application.targetFrameRate;
            }
            else
            {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
                _targetFrameRate = 30f;
#elif UNITY_2021_2_OR_NEWER
                _targetFrameRate = (float)Screen.currentResolution.refreshRateRatio.value;
#else
                _targetFrameRate = (float)Screen.currentResolution.refreshRate;
#endif
            }

            if (_targetFrameRate <= 0)
            {
                _targetFrameRate = 60f;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            _rect = _rectTransform.rect;
        }

        protected override void UpdateGraph(FrameRecorder frameRecorder, Material material)
        {
            // 直近のトータル負荷の最大値を探す
            var currentMaxTotal = 0.001f;
            var samples = frameRecorder.CpuSamples;
            for (var i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                var total = sample.x + sample.y + sample.z + sample.w;
                if (total > currentMaxTotal) currentMaxTotal = total;
            }

            // ターゲット時間（横線）
            var targetMs = 1000f / _targetFrameRate;
            // 縦軸の最大値（ターゲットか、実際の最大負荷の高い方＋余白）
            var dynamicMax = Mathf.Max(targetMs * 1.3f, currentMaxTotal * 1.1f);
            // 設定された最小の最大値（見栄えのため）より小さくしない
            dynamicMax = Mathf.Max(dynamicMax, 10f);
            // 4. Shaderプロパティを一気に更新
            material.SetVectorArray(SamplesId, samples);
            material.SetInt(SampleOffsetId, frameRecorder.SampleOffset);
            material.SetFloat(MaxTimeID, dynamicMax);
            material.SetFloat(TargetTimeID, targetMs);

            _targetLine.SetHeight(_rect.height * (1000f / _targetFrameRate / dynamicMax));
            _targetLine.SetFrameRate(_targetFrameRate);

            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < 1f) return;
            var deltaTimes = frameRecorder.DeltaTimes;
            _fpsText.text = $"{1f / deltaTimes.Average:F2}";
            _avgFrameText.text = $"{deltaTimes.Average * 1000f:F2} ms";
            _maxFrameText.text = $"{deltaTimes.Max * 1000f:F2} ms";
            _elapsed = 0f;
        }
    }
}