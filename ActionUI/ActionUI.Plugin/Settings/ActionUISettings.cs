using ModifAmorphic.Outward.ActionUI.DataModels;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System.IO;

namespace ModifAmorphic.Outward.ActionUI.Settings
{
    internal class ActionUISettings
    {
        public static readonly string PluginPath = Path.GetDirectoryName(ActionUIPlugin.Instance.Info.Location);
        public static readonly string ConfigPath = Path.GetDirectoryName(ActionUIPlugin.Instance.Config.ConfigFilePath);
        public static readonly string CharactersProfilesPath = Path.Combine(ConfigPath, ModInfo.ModId, "profiles");

        public static class ActionViewer
        {
            public const string SkillsTab = "Skills";
            public const string CosmeticsTab = "Cosmetics";
            public const string ConsumablesTab = "Consumables";
            public const string DeployablesTab = "Deployables";
            public const string EquipmentSetsTab = "Equipment Sets";
            public const string EquippedTab = "Equipped";
            public const string ArmorTab = "Armor";
            public const string WeaponsTab = "Weapons";
        }

        public static ActionUIProfile DefaultProfile = new ActionUIProfile()
        {
            Name = "Default",
            ActionSlotsEnabled = true,
            DurabilityDisplayEnabled = true,
            EquipmentSetsEnabled = true,
            SkillChainsEnabled = true,
            EquipmentSetsSettingsProfile = new EquipmentSetsSettingsProfile()
            {
                ArmorSetsInCombatEnabled = false,
                SkipWeaponAnimationsEnabled = false,
                StashEquipEnabled = true,
                StashEquipAnywhereEnabled = false,
                StashUnequipEnabled = false,
                StashUnequipAnywhereEnabled = false,
            },
            StashSettingsProfile = new StashSettingsProfile()
            {
                CharInventoryEnabled = true,
                CharInventoryAnywhereEnabled = false,
                MerchantEnabled = true,
                MerchantAnywhereEnabled = false,
                CraftingInventoryEnabled = true,
                CraftingInventoryAnywhereEnabled = false,
                PreservesFoodEnabled = true,
                PreservesFoodAmount = 75,
            },
            StorageSettingsProfile = new StorageSettingsProfile()
            {
                DisplayCurrencyEnabled = true,
            }
        };

    }
}
