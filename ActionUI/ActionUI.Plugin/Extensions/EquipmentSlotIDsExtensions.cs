using ModifAmorphic.Outward.Unity.ActionUI;
using static EquipmentSlot;

namespace ModifAmorphic.Outward.UI.Extensions
{
    internal static class EquipmentSlotIDsExtensions
    {
        public static EquipmentSlots ToDurableEquipmentSlot(this EquipmentSlotIDs slot)
        {
            switch (slot)
            {
                case EquipmentSlotIDs.Helmet:
                    return EquipmentSlots.Head;
                case EquipmentSlotIDs.Chest:
                    return EquipmentSlots.Chest;
                case EquipmentSlotIDs.LeftHand:
                    return EquipmentSlots.LeftHand;
                case EquipmentSlotIDs.RightHand:
                    return EquipmentSlots.RightHand;
                case EquipmentSlotIDs.Legs:
                    return EquipmentSlots.Feet;
                default:
                    return EquipmentSlots.None;
            }
        }
    }
}
