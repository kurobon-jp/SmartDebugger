using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    [RequireComponent(typeof(Image))]
    internal abstract class GraphShaderBase : MonoBehaviour
    {
        protected static readonly int SamplesId = Shader.PropertyToID("_Samples");
        protected static readonly int SampleCountId = Shader.PropertyToID("_SampleCount");
        protected static readonly int SampleOffsetId = Shader.PropertyToID("_SampleOffset");

        private FrameRecorder _frameRecorder;
        private Material _material;

        protected virtual void Awake()
        {
            _frameRecorder = SmartDebug.Instance.FrameRecorder;
            var image = GetComponent<Image>();
            _material = Instantiate(image.material);
            image.material = _material;
            _material.SetInt(SampleCountId, _frameRecorder.SampleCount);
        }

       private void LateUpdate()
        {
            UpdateGraph(_frameRecorder, _material);
        }

        protected abstract void UpdateGraph(FrameRecorder frameRecorder, Material material);
    }
}