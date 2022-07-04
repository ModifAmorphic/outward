using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Transmorphic.Settings
{
    internal class ConfigSettings
    {
        #region Cooking and Alchemy
        const string GeneralSection = "Cooking and Alchemy";
        const int GeneralTopOrder = int.MaxValue;

        public ConfigSetting<bool> CookingMenuEnabled { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(CookingMenuEnabled),
            DefaultValue = true,
            Section = GeneralSection,
            DisplayName = "Cooking Crafting Menu Enabled",
            Description = $"Enables fully functional Cooking menu in the character / inventory UI.",
            Order = GeneralTopOrder - 1,
            IsAdvanced = false
        };
        public ConfigSetting<bool> DisableCookingKitFuelRequirement { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(DisableCookingKitFuelRequirement),
            DefaultValue = false,
            Section = GeneralSection,
            DisplayName = "  • Disable Extra Ingredients",
            Description = $"Removes the cooking pot and fuel requirement from all recipes in the cooking menu.",
            Order = GeneralTopOrder - 2,
            IsAdvanced = false
        };

        public ConfigSetting<bool> AlchemyMenuEnabled { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(AlchemyMenuEnabled),
            DefaultValue = true,
            Section = GeneralSection,
            DisplayName = "Alchemy Crafting Menu Enabled",
            Description = $"Enables fully functional Alchemy crafting menu in the character / inventory UI.",
            Order = GeneralTopOrder - 3,
            IsAdvanced = false
        };
        public ConfigSetting<bool> DisableAlchemyKitFuelRequirement { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(DisableAlchemyKitFuelRequirement),
            DefaultValue = false,
            Section = GeneralSection,
            DisplayName = "  • Disable Extra Ingredients",
            Description = $"Removes the alchemy kit and fuel requirement from all recipes in the alchemy menu.",
            Order = GeneralTopOrder - 4,
            IsAdvanced = false
        };
        #endregion

#region Enchanting Settings
        const string EnchantingSection = "Enchanting Settings";
        const int EnchantingTopOrder = GeneralTopOrder - 1000;

        public ConfigSetting<bool> EnchantingMenuEnabled { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(EnchantingMenuEnabled),
            DefaultValue = true,
            Section = EnchantingSection,
            DisplayName = "Enchanting Crafting Menu Enabled",
            Description = $"Enables or disables the Enchanting crafting menu.",
            Order = EnchantingTopOrder - 1,
            IsAdvanced = false
        };

        //public ConfigSetting<bool> AllEnchantRecipesLearned { get; } = new ConfigSetting<bool>()
        //{
        //    Name = nameof(AllEnchantRecipesLearned),
        //    DefaultValue = true,
        //    Section = EnchantingSection,
        //    DisplayName = "  • Recipes Unlocked for All Characters",
        //    Description = $"If enabled, all enchantment recipes will automatically be learned by all characters.",
        //    Order = EnchantingTopOrder - 2,
        //    IsAdvanced = false
        //};
        public ConfigSetting<bool> ConditionalEnchantingEnabled{ get; } = new ConfigSetting<bool>()
        {
            Name = nameof(ConditionalEnchantingEnabled),
            DefaultValue = true,
            Section = EnchantingSection,
            DisplayName = "  + Conditional Requirements Enabled",
            Description = $"When enabled, any additional conditions selected from the list below must also be met before" +
            $"an enchanting recipe can be used.",
            Order = EnchantingTopOrder - 3,
            IsAdvanced = false
        };
        public ConfigSetting<bool> EnchantingConditionQuest { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(EnchantingConditionQuest),
            DefaultValue = true,
            Section = EnchantingSection,
            DisplayName = "    • Quest(s) Completed",
            Description = "Quest condition of recipe must be matched. The Enchanting Guild being built is a quest condition as well.",
            Order = EnchantingTopOrder - 4,
            IsAdvanced = false
        };
        public ConfigSetting<bool> EnchantingConditionRegion { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(EnchantingConditionRegion),
            DefaultValue = false,
            Section = EnchantingSection,
            DisplayName = "    • Player in Region",
            Description = "If enabled, player must be in correct region for a recipe (if applicable).",
            Order = EnchantingTopOrder - 5,
            IsAdvanced = false
        };
        public ConfigSetting<bool> EnchantingConditionTime { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(EnchantingConditionTime),
            DefaultValue = false,
            Section = EnchantingSection,
            DisplayName = "    • Time of Day",
            Description = $"For recipes with a specific time of day condition.",
            Order = EnchantingTopOrder - 6,
            IsAdvanced = false
        };
        public ConfigSetting<bool> EnchantingConditionTemperature { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(EnchantingConditionTemperature),
            DefaultValue = false,
            Section = EnchantingSection,
            DisplayName = "    • Temperature",
            Description = "Temperature condition of recipe must be matched.",
            Order = EnchantingTopOrder - 7,
            IsAdvanced = false
        };
        public ConfigSetting<bool> EnchantingConditionWeather { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(EnchantingConditionWeather),
            DefaultValue = false,
            Section = EnchantingSection,
            DisplayName = "    • Weather",
            Description = "Weather condition of recipe must be matched.",
            Order = EnchantingTopOrder - 8,
            IsAdvanced = false
        };
        public ConfigSetting<bool> EnchantingConditionWindAltarState { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(EnchantingConditionWindAltarState),
            DefaultValue = false,
            Section = EnchantingSection,
            DisplayName = "    • Wind Altar Activated",
            Description = $"Wind Altar must be in correct state for a recipe, if applicable.",
            Order = EnchantingTopOrder - 9,
            IsAdvanced = false
        };
        
        #endregion

        #region Transmogrify Settings
        const string TransmogSection = "Transmogrify Settings";
        const int TransmogTopOrder = EnchantingTopOrder - 1000;

        public ConfigSetting<bool> TransmogrifyMenuEnabled { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(TransmogrifyMenuEnabled),
            DefaultValue = true,
            Section = TransmogSection,
            DisplayName = "Transmogrify Crafting Menu Enabled",
            Description = $"Enables or disables the Transmogrify crafting menu. Tranmog'd equipment will continue to function regardless of this setting.",
            Order = TransmogTopOrder - 1,
            IsAdvanced = false
        };

        public ConfigSetting<bool> AllCharactersLearnRecipes { get; } = new ConfigSetting<bool>()
        {
            Name = nameof(AllCharactersLearnRecipes),
            DefaultValue = false,
            Section = TransmogSection,
            DisplayName = "  • Recipes Unlocked for All Characters",
            Description = $"If enabled, when a transmogrify recipe is unlocked by a character it will be made available to all other existing and future characters.",
            Order = TransmogTopOrder - 2,
            IsAdvanced = false
        };
        public ConfigSetting<int> SecondaryTransmogIngredient { get; } = new ConfigSetting<int>()
        {
            Name = nameof(SecondaryTransmogIngredient),
            DefaultValue = 6300030,
            Section = TransmogSection,
            DisplayName = "  • Secondary Transmogrify Ingredient",
            Description = $"Secondary ingredient which is consumed when transmogrifying an item.  Default is a Gold Ingot (6300030). A valid item ID is required. Requires restart before change is applied.",
            Order = TransmogTopOrder - 3,
            IsAdvanced = false
        };

        const string AdvancedSection = "zz--Advanced Settings--zz";
        const int AdvancedTopOrder = TransmogTopOrder - 1000;
        #endregion
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
