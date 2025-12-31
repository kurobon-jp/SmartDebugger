using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace SmartDebugger
{
    public sealed class MemoryGraph : GraphShaderBase
    {
        [SerializeField] private Text _monoText;
        [SerializeField] private Text _totalText;

        private float _refreshTimer;

        private void Update()
        {
            var usedMonoBytes = Profiler.GetMonoUsedSizeLong();
            var totalMonoBytes = Profiler.GetMonoHeapSizeLong();
            var usedBytes = Profiler.GetTotalAllocatedMemoryLong();
            var totalBytes = Profiler.GetTotalReservedMemoryLong();
            var monoNorm = Mathf.Clamp01((float)(usedMonoBytes / (double)totalMonoBytes));
            var totalNorm = Mathf.Clamp01((float)(usedBytes / (double)totalBytes));

            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer >= 0.5f)
            {
                _monoText.text = $"{usedMonoBytes / (1024f * 1024f):F1} / {totalMonoBytes / (1024f * 1024f):F1} MB";
                _totalText.text = $"{usedBytes / (1024f * 1024f):F1} / {totalBytes / (1024f * 1024f):F1} MB";
                _refreshTimer = 0f;
            }

            PushSample(new Vector4(monoNorm, totalNorm, 0, 0));
        }
    }
}