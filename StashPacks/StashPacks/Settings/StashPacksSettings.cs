using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Settings
{
    internal class StashPacksSettings
    {
        const string MainSection = "StashPack Settings";
        const int MainTopOrder = int.MaxValue;


        public ConfigSetting<bool> CraftingFromStashPackItems { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(CraftingFromStashPackItems),
            DefaultValue = false,
            Section = MainSection,
            DisplayName = "Use StashPack Inventory for Crafting",
            Description = $"Enables crafting from any owned StashPacks that are on the ground and linked to their home Stash.",
            Order = MainTopOrder - 1,
            IsAdvanced = false
        };
        public ConfigSetting<bool> AllScenesEnabled { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(AllScenesEnabled),
            DefaultValue = false,
            Section = MainSection,
            DisplayName = "Enable StashPacks for All Scenes",
            Description = $"Enables StashPack functionality for all scenes. Normally only Scenes with Stashes are enabled.",
            Order = MainTopOrder - 2,
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
