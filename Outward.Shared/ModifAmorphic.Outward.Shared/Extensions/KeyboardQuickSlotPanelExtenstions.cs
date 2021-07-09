using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.Extensions
{
    public static class KeyboardQuickSlotPanelExtenstions
    {
        public static float CalculateRectWidth(this QuickSlotPanel keyboardQuickSlotPanel)
        {
            var quickSlotDisplays = keyboardQuickSlotPanel.GetQuickSlotDisplays();
            // Get first two quickslots to calculate margins.
            var matrix = new List<Vector3[]> { new Vector3[4], new Vector3[4] };
            for (int i = 0; i < 2; i++) { quickSlotDisplays[i].RectTransform.GetWorldCorners(matrix[i]); }

            // do some math
            var iconW = matrix[0][2].x - matrix[0][1].x;             // The width of each icon
            var margin = matrix[1][0].x - matrix[0][2].x;            // The margin between each icon
            var elemWidth = iconW + margin;                          // Total space per icon+margin pair
            var totalWidth = elemWidth * quickSlotDisplays.Length; // How long our bar really is

            return totalWidth;
        }
    }
}
