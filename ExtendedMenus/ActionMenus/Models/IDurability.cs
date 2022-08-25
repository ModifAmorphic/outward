using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IDurability
    {
        DurableEquipmentType DurableEquipmentType { get; }
        DurableEquipmentSlot DurableEquipmentSlot { get; }
        List<ColorRange> ColorRanges { get; }
        float MinimumDisplayValue { get; }
        float GetDurabilityRatio();
    }
}
