using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public interface IDurability
    {
        DurableEquipmentType DurableEquipmentType { get; }
        EquipmentSlots DurableEquipmentSlot { get; }
        List<ColorRange> ColorRanges { get; }
        float MinimumDisplayValue { get; }
        float GetDurabilityRatio();
    }
}
