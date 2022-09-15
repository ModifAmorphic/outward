using ModifAmorphic.Outward.Unity.ActionMenus;
using static EquipmentSlot;

namespace ModifAmorphic.Outward.UI.Extensions
{
    internal static class EquipmentExtensions
    {
        public static DurableEquipmentType GetDurableEquipmentType(this Equipment equipment)
        {
            switch (equipment.CurrentEquipmentSlot.SlotType)
            {
                case EquipmentSlotIDs.Helmet:
                    return DurableEquipmentType.Helm;
                case EquipmentSlotIDs.Chest:
                    return DurableEquipmentType.Chest;
                case EquipmentSlotIDs.LeftHand:
                case EquipmentSlotIDs.RightHand:
                    {
                        if (equipment is MeleeWeapon weapon)
                        {
                            if (weapon.Type == Weapon.WeaponType.Shield)
                                return DurableEquipmentType.Shield;
                            else
                                return DurableEquipmentType.MeleeWeapon;
                        }
                        else if (equipment is ProjectileWeapon)
                            return DurableEquipmentType.RangedWeapon;

                        return DurableEquipmentType.None;
                    }
                case EquipmentSlotIDs.Legs:
                    return DurableEquipmentType.Boots;
                default:
                    return DurableEquipmentType.None;
            }
        }
    }
}
