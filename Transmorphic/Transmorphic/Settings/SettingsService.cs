using BepInEx;
using ModifAmorphic.Outward.Config;
using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Transmorphic.Settings
{
    internal class SettingsService
    {
        private readonly ConfigManagerService _configManagerService;
        private readonly ConfigSettingsService _configService;
        private readonly string _minConfigVersion;

#if DEBUG
        private readonly IModifLogger _logger = LoggerFactory.GetLogger(ModInfo.ModId);
#endif
        public SettingsService(BaseUnityPlugin plugin, string minConfigVersion)
        {
#if DEBUG
            _logger.LogTrace($"SettingsService() plugin is {(plugin == null ? "null" : "not null")}. minConfigVersion: {minConfigVersion}");
#endif
            (_configManagerService, _configService, _minConfigVersion) = (new ConfigManagerService(plugin), new ConfigSettingsService(plugin), minConfigVersion);
        }

        public ConfigSettings ConfigureSettings()
        {
            var settings = new ConfigSettings();

            if (!MeetsMinimumVersion(_minConfigVersion))
            {
                _configService.RemoveAllSettings();
            }

            #region Main Section
            //_configService.BindConfigSetting(settings.AllCharactersLearnRecipes, null);

            #endregion

            #region Advanced Section
            //Logging Level
            _configService.BindConfigSetting(settings.LogLevel,
                (SettingValueChangedArgs<LogLevel> args) => LoggerFactory.ConfigureLogger(ModInfo.ModId, ModInfo.ModName, settings.LogLevel.Value));
            LoggerFactory.ConfigureLogger(ModInfo.ModId, ModInfo.ModName, settings.LogLevel.Value);
            //The Version the config was originally created with
            _configService.BindConfigSetting(settings.ConfigVersion, null);

            #endregion

            return settings;
        }
        public EnchantingSettings ConfigureEnchantingSettings(ConfigSettings settings)
        {
            var enchantingSettings = new EnchantingSettings();

            //_configService.BindConfigSetting(settings.AllEnchantRecipesLearned,
            //    (SettingValueChangedArgs<bool> args) => enchantingSettings.AllEnchantRecipesLearned = args.NewValue, true);

            _configService.BindConfigSetting(settings.EnchantingMenuEnabled,
                (SettingValueChangedArgs<bool> args) =>
                {
                    enchantingSettings.EnchantingMenuEnabled = args.NewValue;
                    //_configService.ToggleShowHideAndRefresh(settings.AllEnchantRecipesLearned, enchantingSettings.EnchantingMenuEnabled);
                }, true);



            _configService.BindConfigSetting(settings.EnchantingConditionQuest,
                (SettingValueChangedArgs<bool> args) => enchantingSettings.EnchantingConditionQuest = args.NewValue, true);
            _configService.BindConfigSetting(settings.EnchantingConditionRegion,
                (SettingValueChangedArgs<bool> args) => enchantingSettings.EnchantingConditionRegion = args.NewValue, true);
            _configService.BindConfigSetting(settings.EnchantingConditionTemperature,
                (SettingValueChangedArgs<bool> args) => enchantingSettings.EnchantingConditionTemperature = args.NewValue, true);
            _configService.BindConfigSetting(settings.EnchantingConditionTime,
                (SettingValueChangedArgs<bool> args) => enchantingSettings.EnchantingConditionTime = args.NewValue, true);
            _configService.BindConfigSetting(settings.EnchantingConditionWeather,
                (SettingValueChangedArgs<bool> args) => enchantingSettings.EnchantingConditionWeather = args.NewValue, true);
            _configService.BindConfigSetting(settings.EnchantingConditionWindAltarState,
                (SettingValueChangedArgs<bool> args) => enchantingSettings.EnchantingConditionWindAltarState = args.NewValue, true);


            _configService.BindConfigSetting(settings.ConditionalEnchantingEnabled,
                (SettingValueChangedArgs<bool> args) =>
                {
                    enchantingSettings.ConditionalEnchantingEnabled = args.NewValue;
                    _configService.ToggleShowHideAndRefresh(settings.EnchantingConditionQuest, enchantingSettings.ConditionalEnchantingEnabled);
                    _configService.ToggleShowHideAndRefresh(settings.EnchantingConditionRegion, enchantingSettings.ConditionalEnchantingEnabled);
                    _configService.ToggleShowHideAndRefresh(settings.EnchantingConditionTemperature, enchantingSettings.ConditionalEnchantingEnabled);
                    _configService.ToggleShowHideAndRefresh(settings.EnchantingConditionTime, enchantingSettings.ConditionalEnchantingEnabled);
                    _configService.ToggleShowHideAndRefresh(settings.EnchantingConditionWeather, enchantingSettings.ConditionalEnchantingEnabled);
                    _configService.ToggleShowHideAndRefresh(settings.EnchantingConditionWindAltarState, enchantingSettings.ConditionalEnchantingEnabled);
                }, true);

            return enchantingSettings;
        }
        public TransmogSettings ConfigureTransmogrifySettings(ConfigSettings settings)
        {
            var tmogSettings = new TransmogSettings();

            _configService.BindConfigSetting(settings.AllCharactersLearnRecipes,
                (SettingValueChangedArgs<bool> args) => tmogSettings.AllCharactersLearnRecipes = args.NewValue, true);

            _configService.BindConfigSetting(settings.TransmogrifyMenuEnabled,
                (SettingValueChangedArgs<bool> args) =>
                {
                    tmogSettings.TransmogMenuEnabled = args.NewValue;
                    _configService.ToggleShowHideAndRefresh(settings.AllCharactersLearnRecipes, tmogSettings.TransmogMenuEnabled);
                }, true);

            return tmogSettings;
        }
        public CookingSettings ConfigureCookingSettings(ConfigSettings settings)
        {
            var cookingSettings = new CookingSettings();

            _configService.BindConfigSetting(settings.DisableCookingKitFuelRequirement,
                (SettingValueChangedArgs<bool> args) => cookingSettings.DisableKitFuelRequirement = args.NewValue, true);

            _configService.BindConfigSetting(settings.CookingMenuEnabled,
                (SettingValueChangedArgs<bool> args) =>
                {
                    cookingSettings.CookingMenuEnabled = args.NewValue;
                    _configService.ToggleShowHideAndRefresh(settings.DisableCookingKitFuelRequirement, cookingSettings.CookingMenuEnabled);
                }, true);

            return cookingSettings;
        }
        public AlchemySettings ConfigureAlchemySettings(ConfigSettings settings)
        {
            var alchemySettings = new AlchemySettings();

            _configService.BindConfigSetting(settings.DisableAlchemyKitFuelRequirement,
                (SettingValueChangedArgs<bool> args) => alchemySettings.DisableKitFuelRequirement = args.NewValue, true);

            _configService.BindConfigSetting(settings.AlchemyMenuEnabled,
                (SettingValueChangedArgs<bool> args) =>
                {
                    alchemySettings.AlchemyMenuEnabled = args.NewValue;
                    _configService.ToggleShowHideAndRefresh(settings.DisableAlchemyKitFuelRequirement, alchemySettings.AlchemyMenuEnabled);
                }, true);

            return alchemySettings;
        }
        private bool MeetsMinimumVersion(string minimumVersion)
        {
            var configVersionValue = _configService.PeekSavedConfigValue(new ConfigSettings().ConfigVersion);
            if (string.IsNullOrWhiteSpace(configVersionValue))
            {
                return false;
            }

            if (!Version.TryParse(configVersionValue, out var configVersion)
                || !Version.TryParse(minimumVersion, out var minVersion))
            {
                return false;
            }
#if DEBUG
            _logger.LogDebug($"Current Config {nameof(MeetsMinimumVersion)}? {configVersion.CompareTo(minVersion) >= 0}. Compared: " +
                   $"ConfigVersion: {configVersion} to MinimumVersion: {minVersion}");
#endif
            return configVersion.CompareTo(minVersion) >= 0;
        }
    }
}
