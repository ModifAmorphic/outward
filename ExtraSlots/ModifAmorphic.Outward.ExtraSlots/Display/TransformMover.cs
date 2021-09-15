using ModifAmorphic.Outward.Logging;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ExtraSlots.Display
{
    internal static class TransformMover
    {
        private static IModifLogger _logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static void SetPosition(Transform transform, Vector3 newPosition)
        {
            var before = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            transform.position = new Vector3(newPosition.x, newPosition.y, newPosition.z);
            _logger.LogTrace($"{nameof(SetPosition)}: Set position of transform {transform.name}" +
                $"\n\tBefore  - [x={before.x} ; y={before.y}; z={before.z}]" +
                $"\n\tDesired - [x={newPosition.x} ; y={newPosition.y}; z={newPosition.z}]" + 
                $"\n\tAfter   - [x={transform.position.x} ; y={transform.position.y}; z={transform.position.z}]");
        }
        public static void SetRectPosition(RectTransform transform, Vector3 leftCornerPosition)
        {
            //Offset the X position by half, because we're using the left corner as a reference and transform.X is the middle.
            //Without this offset, a X position of zero would position the quickbar where half of it is off the left side of the screen
            var offsetX = leftCornerPosition.x + (transform.rect.width / 2f);

            var before = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            transform.position = new Vector3(offsetX, leftCornerPosition.y, leftCornerPosition.z);
            _logger.LogTrace($"{nameof(SetPosition)}: Set position of transform {transform.name}" +
                $"\n\tBefore  - [x={before.x} ; y={before.y}; z={before.z}]" +
                $"\n\tDesired - [x={leftCornerPosition.x} ; y={leftCornerPosition.y}; z={leftCornerPosition.z}]" +
                $"\n\tAfter   - [x={transform.position.x} ; y={transform.position.y}; z={transform.position.z}]");
        }
        public static void MoveAbove(RectTransform refRectTransform, RectTransform targetRectTransform, float yOffset)
        {
            var refCorners = new Vector3[4];
            refRectTransform.GetWorldCorners(refCorners);

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
            targetRectTransform.GetWorldCorners(targetCornersAfter);
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
