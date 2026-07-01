using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    internal class HorizontalWrapLayoutGroup : LayoutGroup
    {
        [SerializeField] private Vector2 _spacing;

        public override void CalculateLayoutInputVertical()
        {
            var height = CalculateLayout();
            SetLayoutInputForAxis(height, height, -1, 1);
        }

        public override void SetLayoutHorizontal() => Layout();
        public override void SetLayoutVertical() => Layout();

        private void Layout()
        {
            CalculateLayout(doLayout: true);
        }

        private float CalculateLayout(bool doLayout = false)
        {
            var width = rectTransform.rect.width;
            float x = padding.left;
            float y = padding.top;
            var rowHeight = 0f;
            var hasPendingLineBreak = false; // ← width=0 の連続区間を１つの改行として扱う
            var column = 0;
            for (var i = 0; i < rectChildren.Count; i++)
            {
                var child = rectChildren[i];
                var w = child.rect.width;
                var h = child.rect.height;

                // -------------------------
                //  🔵 0 サイズ → 連続チェックで 1 改行だけ
                // -------------------------
                if (w == 0f)
                {
                    hasPendingLineBreak = true;
                    continue;
                }

                column++;
                // 🔵 0 サイズ子が連続していた場合、ここで 1 回だけ改行
                if (hasPendingLineBreak)
                {
                    y += rowHeight + (column > 1 ? _spacing.y : 0);
                    x = padding.left;
                    rowHeight = 0f;
                    hasPendingLineBreak = false;
                }

                // -------------------------
                //  🔵 通常の溢れによる改行
                // -------------------------
                if (x + w + padding.right > width)
                {
                    y += rowHeight + _spacing.y;
                    x = padding.left;
                    rowHeight = 0f;
                }

                // -------------------------
                //  🔵 配置
                // -------------------------
                if (doLayout)
                {
                    SetChild(child, x, y, w, h);
                }

                x += w + _spacing.x;
                rowHeight = Mathf.Max(rowHeight, h);
            }

            // 終端に 0 サイズ改行が続いていても無視（すでに行末のため）

            return y + rowHeight + padding.bottom;
        }

        private void SetChild(RectTransform child, float x, float y, float w, float h)
        {
            m_Tracker.Add(this, child,
                DrivenTransformProperties.AnchoredPosition |
                DrivenTransformProperties.Anchors |
                DrivenTransformProperties.Pivot);

            child.anchorMin = child.anchorMax = new Vector2(0, 1);
            child.pivot = new Vector2(0, 1);
            child.anchoredPosition = new Vector2(x, -y);
        }
    }
}