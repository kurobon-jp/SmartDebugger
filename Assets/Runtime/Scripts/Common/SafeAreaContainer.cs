using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    internal class SafeAreaContainer : Graphic
    {
        [SerializeField] private RectTransform _content;

        private ScreenOrientation _orientation;
        private int _screenWidth;
        private int _screenHeight;
        private Rect _safeArea;

        protected override void OnEnable()
        {
            base.OnEnable();
            Resize();
        }

        private void Update()
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            var safeArea = Screen.safeArea;
            if (screenWidth <= 0 || screenHeight <= 0) return;

            if (_orientation == Screen.orientation &&
                _screenWidth == screenWidth &&
                _screenHeight == screenHeight &&
                _safeArea == safeArea) return;

            if (!Resize()) return;

            _orientation = Screen.orientation;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _safeArea = safeArea;
        }

        private bool Resize()
        {
            if (_content == null) return false;
            if (Screen.width <= 0 || Screen.height <= 0) return false;

            var safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            anchorMin.x = Mathf.Clamp01(anchorMin.x / Screen.width);
            anchorMin.y = Mathf.Clamp01(anchorMin.y / Screen.height);
            anchorMax.x = Mathf.Clamp01(anchorMax.x / Screen.width);
            anchorMax.y = Mathf.Clamp01(anchorMax.y / Screen.height);

            if (!IsFinite(anchorMin) || !IsFinite(anchorMax)) return false;

            _content.sizeDelta = Vector2.zero;
            _content.anchorMin = anchorMin;
            _content.anchorMax = anchorMax;
            _content.offsetMin = Vector2.zero;
            _content.offsetMax = Vector2.zero;

            SetVerticesDirty();
            return true;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var safe = Screen.safeArea;
            float w = Screen.width;
            float h = Screen.height;
            if (w <= 0 || h <= 0) return;

            var s = new Vector2(rectTransform.rect.width / w, rectTransform.rect.height / h);
            if (!IsFinite(s)) return;

            // 8つの頂点を定義（外側の枠 4つ + 内側の穴 4つ）
            // 外枠 (0~3)
            AddVert(vh, 0, 0, s); // 左下
            AddVert(vh, 0, h, s); // 左上
            AddVert(vh, w, h, s); // 右上
            AddVert(vh, w, 0, s); // 右下

            // 内穴 (4~7)
            AddVert(vh, safe.xMin, safe.yMin, s); // 左下
            AddVert(vh, safe.xMin, safe.yMax, s); // 左上
            AddVert(vh, safe.xMax, safe.yMax, s); // 右上
            AddVert(vh, safe.xMax, safe.yMin, s); // 右下

            // 4つの四角形（メッシュ）を張る
            // 左側
            vh.AddTriangle(0, 1, 5);
            vh.AddTriangle(0, 5, 4);
            // 上側
            vh.AddTriangle(1, 2, 6);
            vh.AddTriangle(1, 6, 5);
            // 右側
            vh.AddTriangle(2, 3, 7);
            vh.AddTriangle(2, 7, 6);
            // 下側
            vh.AddTriangle(3, 0, 4);
            vh.AddTriangle(3, 4, 7);
        }

        private void AddVert(VertexHelper vh, float x, float y, Vector2 scale)
        {
            var v = UIVertex.simpleVert;
            v.color = color;
            // RectTransformの中心位置を考慮して座標をオフセット
            v.position = new Vector3(x * scale.x - rectTransform.pivot.x * rectTransform.rect.width,
                y * scale.y - rectTransform.pivot.y * rectTransform.rect.height);
            vh.AddVert(v);
        }

        private static bool IsFinite(Vector2 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                   !float.IsNaN(value.y) && !float.IsInfinity(value.y);
        }
    }
}
