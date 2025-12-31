using UnityEngine;

namespace SmartDebugger
{
    public abstract class BaseField : BaseView
    {
        public void SetWidth(float width)
        {
            var rectTransform = (RectTransform) transform;
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
        }
    }
}