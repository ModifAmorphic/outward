using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.RespecPotions.Settings
{
    internal class RespecConfigSettings
    {
        const string MainSection = "StashPack Settings";
        const int MainTopOrder = int.MaxValue;

        //public ConfigSetting<bool> ConfigSettingValue { get; } = new ConfigSetting<bool>()
        //{
        //    Name = nameof(ConfigSettingValue),
        //    DefaultValue = true,
        //    Section = MainSection,
        //    DisplayName = "Config Setting Value Example",
        //    Description = $"Example of config setting.",
        //    Order = MainTopOrder - 1,
        //    IsAdvanced = false
        //};

        const string HiddenSection = "*Hidden* Settings";
        const int HiddenTopOrder = MainTopOrder - 1000;
        public ConfigSetting<int> PotionValue { get; } = new ConfigSetting<int>()
        {
            Name = nameof(PotionValue),
            DefaultValue = 500,
            Section = HiddenSection,
            DisplayName = "Base Value of Potions",
            Description = $"Base value of Potions for selling, store prices.",
            Order = HiddenTopOrder - 1,
            IsAdvanced = false,
        };
        public ConfigSetting<int> ShopMinAmount { get; } = new ConfigSetting<int>()
        {
            Name = nameof(ShopMinAmount),
            DefaultValue = 3,
            Section = HiddenSection,
            DisplayName = "Shop potion minimum",
            Description = $"Minimum amount of each forget school potion an alchemist shop will carry.",
            Order = HiddenTopOrder - 2,
            IsAdvanced = false,
        };
        public ConfigSetting<int> ShopMaxAmount { get; } = new ConfigSetting<int>()
        {
            Name = nameof(ShopMaxAmount),
            DefaultValue = 3,
            Section = HiddenSection,
            DisplayName = "Shop potion maximum",
            Description = $"Maximum amount of each forget school potion an alchemist shop will carry.",
            Order = HiddenTopOrder - 3,
            IsAdvanced = false,
        };


        const string AdvancedSection = "zz--Advanced Settings--zz";
        const int AdvancedTopOrder = HiddenTopOrder - 1000;

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
