using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Settings
{
    internal class StashPacksSettings
    {
        const string AdvancedSection = "zz--Advanced Settings--zz";
        const int AdvancedTopOrder = 1000;

        public ConfigSetting<LogLevel> LogLevel { get; } = new ConfigSetting<LogLevel>()
        {
            Name = nameof(LogLevel),
            DefaultValue = Logging.LogLevel.Info,
            Section = AdvancedSection,
            DisplayName = "Minimum level for logging",
            Description = $"The threshold for logging events to the UnityEngine.Debug logger. " +
        $"{Enum.GetName(typeof(LogLevel), Logging.LogLevel.Info)} is the default.",
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
