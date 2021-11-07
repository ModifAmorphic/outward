using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward
{
    public static class ItemTags
    {
        private static Tag _equipmentTag;
        public static Tag EquipmentTag
        {
            get
            {
                if (!_equipmentTag.IsSet)
                    _equipmentTag = TagSourceManager.Instance.GetTag("13");
                return _equipmentTag;
            }
        }
        private static Tag _weaponTag;
        public static Tag WeaponTag
        {
            get
            {
                if (!_weaponTag.IsSet)
                    _weaponTag = TagSourceManager.Instance.GetTag("1");
                return _weaponTag;
            }
        }
        private static Tag _bowTag;
        public static Tag BowTag
        {
            get
            {
                if (!_bowTag.IsSet)
                    _bowTag = TagSourceManager.Instance.GetTag("8");
                return _bowTag;
            }
        }
        private static Tag _pistolTag;
        public static Tag PistolTag
        {
            get
            {
                if (!_pistolTag.IsSet)
                    _pistolTag = TagSourceManager.Instance.GetTag("21");
                return _pistolTag;
            }
        }
        private static Tag _chakramTag;
        public static Tag ChakramTag
        {
            get
            {
                if (!_chakramTag.IsSet)
                    _chakramTag = TagSourceManager.Instance.GetTag("160");
                return _chakramTag;
            }
        }
        private static Tag _helmetTag;
        public static Tag HelmetTag
        {
            get
            {
                if (!_helmetTag.IsSet)
                    _helmetTag = TagSourceManager.Instance.GetTag("14");
                return _helmetTag;
            }
        }
        private static Tag _armorTag;
        public static Tag ArmorTag
        {
            get
            {
                if (!_armorTag.IsSet)
                    _armorTag = TagSourceManager.Instance.GetTag("15");
                return _armorTag;
            }
        }
        private static Tag _bootsTag;
        public static Tag BootsTag
        {
            get
            {
                if (!_bootsTag.IsSet)
                    _bootsTag = TagSourceManager.Instance.GetTag("16");
                return _bootsTag;
            }
        }
        private static Tag _dagueTag;
        public static Tag DagueTag
        {
            get
            {
                if (!_dagueTag.IsSet)
                    _dagueTag = TagSourceManager.Instance.GetTag("22");
                return _dagueTag;
            }
        }
        private static Tag _trinketTag;
        public static Tag TrinketTag
        {
            get
            {
                if (!_trinketTag.IsSet)
                    _trinketTag = TagSourceManager.Instance.GetTag("17");
                return _trinketTag;
            }
        }
        private static Tag _lanternTag;
        public static Tag LanternTag
        {
            get
            {
                if (!_lanternTag.IsSet)
                    _lanternTag = TagSourceManager.Instance.GetTag("37");
                return _lanternTag;
            }
        }
        private static Tag _lexiconTag;
        public static Tag LexiconTag
        {
            get
            {
                if (!_lexiconTag.IsSet)
                    _lexiconTag = TagSourceManager.Instance.GetTag("161");
                return _lexiconTag;
            }
        }
        private static Tag _backpackTag;
        public static Tag BackpackTag
        {
            get
            {
                if (!_backpackTag.IsSet)
                    _backpackTag = TagSourceManager.Instance.GetTag("68");
                return _backpackTag;
            }
        }
        private static Tag _enchantIngredientsTag;
        public static Tag EnchantIngredientsTag
        {
            get
            {
                if (!_enchantIngredientsTag.IsSet)
                    _enchantIngredientsTag = TagSourceManager.Instance.GetTag("54");
                return _enchantIngredientsTag;
            }
        }
    }
}
