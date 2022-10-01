using ModifAmorphic.Outward.Unity.ActionUI;
using static EquipmentSlot;

namespace ModifAmorphic.Outward.ActionUI.Extensions
{
    internal static class EquipmentSlotIDsExtensions
    {
        public static EquipSlots ToDurableEquipmentSlot(this EquipmentSlotIDs slot)
        {
            switch (slot)
            {
                case EquipmentSlotIDs.Helmet:
                    return EquipSlots.Head;
                case EquipmentSlotIDs.Chest:
                    return EquipSlots.Chest;
                case EquipmentSlotIDs.LeftHand:
                    return EquipSlots.LeftHand;
                case EquipmentSlotIDs.RightHand:
                    return EquipSlots.RightHand;
                case EquipmentSlotIDs.Foot:
                    return EquipSlots.Feet;
                default:
                    return EquipSlots.None;
            }
        }
    }
}
