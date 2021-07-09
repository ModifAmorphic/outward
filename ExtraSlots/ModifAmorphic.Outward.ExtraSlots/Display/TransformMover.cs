using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ExtraSlots.Display
{
    internal class TransformMover
    {
        private readonly Logging.Logger _logger;

        public TransformMover(Logging.Logger logger)
        {
            this._logger = logger;
        }
        public static void SetPosition(Transform transform, Vector3 newPosition)
        {
            transform.position = new Vector3(newPosition.x, newPosition.y, newPosition.z);
        }
        public void MoveAbove(RectTransform refRectTransform, RectTransform targetRectTransform, float yOffset)
        {
            var refCorners = new Vector3[4];
            refRectTransform.GetWorldCorners(refCorners = new Vector3[4]);

            var targetCorners = new Vector3[4];
            targetRectTransform.GetWorldCorners(targetCorners);

            //calculate move distance between the top of the stability rectangle and the bottom of the panel rectangle
            var moveDistance = refCorners[1].y - targetCorners[0].y + yOffset;
            _logger.LogTrace($"Before {nameof(TransformMover)}.{nameof(MoveAbove)}(): Reference Rect {refRectTransform.name}- Upper Left Corner Pos [x={refCorners[1].x} ; y={refCorners[1].y}; z={refCorners[1].z}]" +
                                $"\n\tTarget Rect {targetRectTransform.name} - Lower Left Corner Pos [x={targetCorners[0].x} ; y={targetCorners[0].y}; z={targetCorners[0].z}]");
            _logger.LogDebug($"{nameof(TransformMover)}.{nameof(MoveAbove)}(): Moving Target Rect {targetRectTransform.name} above Reference Rect {refRectTransform.name}. Old [y={targetRectTransform.position.y}] | New [y={targetRectTransform.position.y + moveDistance}]");

            var originalPos = new Vector3(targetRectTransform.position.x, targetRectTransform.position.y, targetRectTransform.position.z);
            targetRectTransform.position = new Vector3(targetRectTransform.position.x, targetRectTransform.position.y + moveDistance, targetRectTransform.position.z);
            //Log result of transform
            var targetCornersAfter = new Vector3[4];
            targetRectTransform.GetWorldCorners(targetCornersAfter = new Vector3[4]);
            _logger.LogTrace($"After {nameof(TransformMover)}.{nameof(MoveAbove)}(): Reference Rect {refRectTransform.name}- Upper Left Corner Pos [x={refCorners[1].x} ; y={refCorners[1].y}; z={refCorners[1].z}]" +
                                $"\n\tTarget Rect {targetRectTransform.name} - Lower Left Corner Pos [x={targetCornersAfter[0].x} ; y={targetCornersAfter[0].y}; z={targetCornersAfter[0].z}]");
        }
#if DEBUG
        private static void DebugChildren(Transform transform, Logging.Logger logger, bool recursive = false)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(DebugChildren)}: Parent '{transform.name}'");
            for (int c = 0; c < transform.childCount; c++)
            {
                var child = transform.GetChild(c);
                int recursion = 0;
                sb.AppendLine($"  {child.name} children: {child.childCount}");
                DebugFirstChild(child, sb, true, ref recursion);
            }
            logger.LogDebug(sb.ToString());
        }
        private static void DebugFirstChild(Transform transform, StringBuilder sb, bool recursive, ref int recursion)
        {
            string indent = string.Empty;
            for (int i = 0; i < recursion + 2; i++)
                indent += "  ";
            if (transform.childCount > 0)
            {
                var child = transform.GetChild(0);
                sb.AppendLine($"{indent}{child.name}");
                if (recursive)
                {
                    recursion++;
                    DebugFirstChild(child, sb, true, ref recursion);
                }
            }
        }
#endif
    }
}
