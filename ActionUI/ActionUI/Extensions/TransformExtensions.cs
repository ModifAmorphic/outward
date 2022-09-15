using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Extensions
{
    public static class TransformExtensions
    {
        public static string GetPath(this Transform transform)
        {
            var path = transform.gameObject.name;
            var parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return "/" + path;
        }

        public static UIPosition ToRectTransformPosition(this RectTransform rectTransform)
        {
            return new UIPosition()
            {
                Position = rectTransform.position.ToUIPosition2D(),
                AnchoredPosition = rectTransform.anchoredPosition.ToUIPosition2D(),
                AnchoredMin = rectTransform.anchorMin.ToUIPosition2D(),
                AnchoredMax = rectTransform.anchorMax.ToUIPosition2D(),
                OffsetMin = rectTransform.offsetMin.ToUIPosition2D(),
                OffsetMax = rectTransform.offsetMax.ToUIPosition2D(),
                Pivot = rectTransform.pivot.ToUIPosition2D()
            };
        }
    }
}
