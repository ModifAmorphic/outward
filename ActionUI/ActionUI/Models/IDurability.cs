using System.Collections.Generic;

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
