using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;
using static EquipmentSlot;

namespace ModifAmorphic.Outward.UI.Extensions
{
    internal static class EquipmentSlotIDsExtensions
    {
        public static DurableEquipmentSlot ToDurableEquipmentSlot(this EquipmentSlotIDs slot)
        {
            switch (slot)
            {
                case EquipmentSlotIDs.Helmet:
                    return DurableEquipmentSlot.Head;
                case EquipmentSlotIDs.Chest:
                    return DurableEquipmentSlot.Chest;
                case EquipmentSlotIDs.LeftHand:
                    return DurableEquipmentSlot.LeftHand;
                case EquipmentSlotIDs.RightHand:
                    return DurableEquipmentSlot.RightHand;
                case EquipmentSlotIDs.Legs:
                    return DurableEquipmentSlot.Feet;
                default:
                    return DurableEquipmentSlot.None;
            }
        }
    }
}
