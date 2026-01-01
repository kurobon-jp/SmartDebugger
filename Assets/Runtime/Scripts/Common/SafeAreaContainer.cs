using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public class SafeAreaContainer : Graphic
    {
        [SerializeField] private RectTransform _content;

        private ScreenOrientation _orientation;
        private Resolution _resolution;

        protected override void OnEnable()
        {
            base.OnEnable();
            Resize();
        }

        private void Update()
        {
            if (_orientation == Screen.orientation &&
                _resolution.width == Screen.currentResolution.width &&
                _resolution.height == Screen.currentResolution.height) return;
            _orientation = Screen.orientation;
            _resolution = Screen.currentResolution;
            Resize();
        }

        private void Resize()
        {
            if (_content == null) return;
            var safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _content.anchorMin = anchorMin;
            _content.anchorMax = anchorMax;
            _content.offsetMin = Vector2.zero;
            _content.offsetMax = Vector2.zero;

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var safe = Screen.safeArea;
            float w = Screen.width;
            float h = Screen.height;
            var s = new Vector2(rectTransform.rect.width / w, rectTransform.rect.height / h);

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
    }
}