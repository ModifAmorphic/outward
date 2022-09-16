using ModifAmorphic.Outward.UI.DataModels;
using System.IO;

namespace ModifAmorphic.Outward.UI.Settings
{
    internal class ActionUISettings
    {
        public const string GameObjectName = "MenuOverhaul";

        public static readonly string PluginPath = Path.GetDirectoryName(ActionUIPlugin.Instance.Info.Location);
        public static readonly string ConfigPath = Path.GetDirectoryName(ActionUIPlugin.Instance.Config.ConfigFilePath);
        public static readonly string ProfilesPath = Path.Combine(ConfigPath, ModInfo.ModId, "profiles");

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
        };

    }
}
