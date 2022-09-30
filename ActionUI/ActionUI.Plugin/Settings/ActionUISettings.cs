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
            public const string ConsumablesTab = "Consumables";
            public const string DeployablesTab = "Deployables";
            public const string EquippedTab = "Equipped";
            public const string ArmorTab = "Armor";
            public const string WeaponsTab = "Weapons";
        }

        public static ActionUIProfile DefaultProfile = new ActionUIProfile()
        {
            Name = "Profile 1",
            ActionSlotsEnabled = true,
            DurabilityDisplayEnabled = true,
            StashCraftingEnabled = true,
            CraftingOutsideTownEnabled = false,
            EquipmentSetsEnabled = true,
            EquipmentSetsSettingsProfile = new EquipmentSetsSettingsProfile()
            {
                ArmorSetsInCombatEnabled = false,
                SkipWeaponAnimationsEnabled = false,
                StashEquipEnabled = true,
                StashEquipAnywhereEnabled = false,
                StashUnequipEnabled = false,
                StashUnequipAnywhereEnabled = false,
            }
        };

    }
}
