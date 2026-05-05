using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    internal sealed class MemoryGraph : GraphShaderBase
    {
        [SerializeField] private Text _monoText;
        [SerializeField] private Text _totalText;

        private float _elapsed;

        protected override void UpdateGraph(FrameRecorder frameRecorder, Material material)
        {
            material.SetVectorArray(SamplesId, frameRecorder.MemorySamples);
            material.SetInt(SampleOffsetId, frameRecorder.SampleOffset);

            _elapsed += Time.unscaledDeltaTime;
            if (!(_elapsed >= 0.5f)) return;
            var usedMonoBytes = frameRecorder.UsedMonoBytes;
            var totalMonoBytes = frameRecorder.TotalMonoBytes;
            var usedBytes = frameRecorder.UsedBytes;
            var totalBytes = frameRecorder.TotalBytes;
            _monoText.text = $"{usedMonoBytes / (1024f * 1024f):F1} / {totalMonoBytes / (1024f * 1024f):F1} MB";
            _totalText.text = $"{usedBytes / (1024f * 1024f):F1} / {totalBytes / (1024f * 1024f):F1} MB";
            _elapsed = 0f;
        }
    }
}