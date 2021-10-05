using ModifAmorphic.Outward.Transmorph.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ModifAmorphic.Outward.Extensions;

namespace ModifAmorphic.Outward.Transmorph.Extensions
{
    public static class TagExtensions
    {
        public static Tag ToWeaponTag(this Weapon.WeaponType weaponType)
        {
            var tag = new Tag(
                (TransmogSettings.WeaponTagStartUID - (int)weaponType).ToString(), 
                "TransmogWeapon");
            tag.SetTagType(Tag.TagTypes.Weapons);
            return tag;
        }
        public static Tag ToArmorTag(this EquipmentSlot.EquipmentSlotIDs equipmentSlot)
        {
            var tag = new Tag(
                (TransmogSettings.ArmorTagStartUID - (int)equipmentSlot).ToString(),
                "TransmogArmor");
            tag.SetTagType(Tag.TagTypes.Armor);
            return tag;
        }
        public static bool TryGetWeaponType(this Tag tag, out Weapon.WeaponType weaponType)
        {
            weaponType = (Weapon.WeaponType)(-1);
            if (tag.GetTagType() != Tag.TagTypes.Weapons 
                    || !int.TryParse(tag.UID, out var uid))
                return false;

            var typeNo = (uid - TransmogSettings.WeaponTagStartUID) * (-1);
            if (!Enum.IsDefined(typeof(Weapon.WeaponType), typeNo))
                return false;

            weaponType = (Weapon.WeaponType)typeNo;
            return true;
        }
        public static bool TryGetEquipmentSlot(this Tag tag, out EquipmentSlot.EquipmentSlotIDs equipmentSlot)
        {
            equipmentSlot = (EquipmentSlot.EquipmentSlotIDs)(-1);
            if (tag.GetTagType() != Tag.TagTypes.Armor 
                    || !int.TryParse(tag.UID, out var uid))
                return false;

            var slotNo = (uid - TransmogSettings.ArmorTagStartUID) * (-1);
            if (!Enum.IsDefined(typeof(EquipmentSlot.EquipmentSlotIDs), slotNo))
                return false;

            equipmentSlot = (EquipmentSlot.EquipmentSlotIDs)slotNo;
            return true;
        }
        public static Tag.TagTypes GetTagType(this Tag tag)
        {
            return tag.GetPrivateField<Tag, Tag.TagTypes>("m_tagType");
        }
    }
}
