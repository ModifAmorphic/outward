using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Settings
{
    internal class StashPacksConfigSettings
    {
        const string MainSection = "StashPack Settings";
        const int MainTopOrder = int.MaxValue;

        public ConfigSetting<bool> PreferPickupToPouch { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(PreferPickupToPouch),
            DefaultValue = true,
            Section = MainSection,
            DisplayName = "Prefer pickup to pouch",
            Description = $"When picking up a StashPack, prefer that the bag is placed in the pouch first.",
            Order = MainTopOrder - 1,
            IsAdvanced = false
        };
        public ConfigSetting<bool> CraftingFromStashPackItems { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(CraftingFromStashPackItems),
            DefaultValue = true,
            Section = MainSection,
            DisplayName = "Use StashPack Inventory for Crafting",
            Description = $"Enables crafting from any owned StashPacks that are on the ground and linked to their home Stash.",
            Order = MainTopOrder - 2,
            IsAdvanced = false
        };
        public ConfigSetting<bool> AllScenesEnabled { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(AllScenesEnabled),
            DefaultValue = false,
            Section = MainSection,
            DisplayName = "Enable StashPacks for All Scenes",
            Description = $"Enables StashPack functionality for all scenes. Normally only Scenes with Stashes are enabled.",
            Order = MainTopOrder - 3,
            IsAdvanced = false
        };

        public ConfigSetting<bool> DisableBagScalingRotation { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(DisableBagScalingRotation),
            DefaultValue = false,
            Section = MainSection,
            DisplayName = "Disable Scaling & Rotation of StashPacks",
            Description = $"Disables scaling of StashPacks to larger than a regular bag and rotating them so they land standing up.",
            Order = MainTopOrder - 4,
            IsAdvanced = false
        };


        const string AdvancedSection = "zz--Advanced Settings--zz";
        const int AdvancedTopOrder = MainTopOrder - 1000;

        public ConfigSetting<LogLevel> LogLevel { get; } = new ConfigSetting<LogLevel>()
        {
            Name = nameof(LogLevel),
            DefaultValue = Logging.LogLevel.Info,
            Section = AdvancedSection,
            DisplayName = "Minimum level for logging",
            Description = $"The threshold for logging events to the UnityEngine.Debug logger. " +
        $"{Enum.GetName(typeof(LogLevel), Logging.LogLevel.Debug)} is the default.",
            Order = AdvancedTopOrder - 1,
            IsAdvanced = true
        };

        public ConfigSetting<string> ConfigVersion { get; } = new ConfigSetting<string>()
        {
            Name = nameof(ConfigVersion),
            DefaultValue = ModInfo.ModVersion,
            Section = AdvancedSection,
            DisplayName = "Created with version",
            Description = $"The version of StashPacks this configuration file was created for.  **Warning - Changing this could result in resetting all config values.**",
            Order = AdvancedTopOrder - 2,
            IsAdvanced = true
        };
    }
}
