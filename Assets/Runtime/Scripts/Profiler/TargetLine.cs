using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartDebugger
{
    internal class TargetLine : UIBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Text _frameRate;

        internal void SetFrameRate(float frameRate)
        {
            _frameRate.text = $"{(int)frameRate}FPS";
        }

        internal void SetHeight(float height)
        {
            _rectTransform.anchoredPosition = new Vector2(0f, height);
        }
    }
}