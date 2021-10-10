using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Transmorph.Settings
{
    internal class TransmorphConfigSettings
    {
        const string TransmogSection = "Transmogrify Settings";
        const int TransmogTopOrder = int.MaxValue;

        public ConfigSetting<string> SomeStringSetting { get; } = new ConfigSetting<string>()
        {
            Name = nameof(SomeStringSetting),
            DefaultValue = "default",
            Section = TransmogSection,
            DisplayName = "SomeStringSetting Friendly Name",
            Description = $"This is Some String Setting Example",
            Order = TransmogTopOrder - 1,
            IsAdvanced = false
        };
        public ConfigSetting<bool> AllCharactersLearnRecipes { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(AllCharactersLearnRecipes),
            DefaultValue = false,
            Section = TransmogSection,
            DisplayName = "Recipes Unlocked for All Characters",
            Description = $"If enabled, when a transmogrify recipe is unlocked by a character it will be made available to all other existing and future characters.",
            Order = TransmogTopOrder - 2,
            IsAdvanced = false
        };


        const string AdvancedSection = "zz--Advanced Settings--zz";
        const int AdvancedTopOrder = TransmogTopOrder - 1000;

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
            Description = $"The version of {ModInfo.ModName} this configuration file was created for.  **Warning - Changing this could result in resetting all config values.**",
            Order = AdvancedTopOrder - 2,
            IsAdvanced = true
        };
    }
}
