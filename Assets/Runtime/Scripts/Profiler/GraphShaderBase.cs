using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    [RequireComponent(typeof(Image))]
    public abstract class GraphShaderBase : MonoBehaviour
    {
        private static readonly int Samples = Shader.PropertyToID("_Samples");
        private static readonly int SampleCount = Shader.PropertyToID("_SampleCount");
        [SerializeField] protected int _sampleCount = 180;

        private Vector4[] _samples;
        private Material _material;

        protected virtual void Awake()
        {
            _samples = new Vector4[_sampleCount];
            var image = GetComponent<Image>();
            _material = Instantiate(image.material);
            image.material = _material;
            _material.SetInt(SampleCount, _sampleCount);
        }

        protected void PushSample(Vector4 v)
        {
            for (int i = _sampleCount - 1; i > 0; i--)
                _samples[i] = _samples[i - 1];

            _samples[0] = v;
            _material.SetVectorArray(Samples, _samples);
        }
    }
}