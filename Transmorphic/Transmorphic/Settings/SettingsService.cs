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

        public TransmorphConfigSettings ConfigureSettings()
        {
            var settings = new TransmorphConfigSettings();

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
        public GlobalSettings ConfigureGlobalSettings(TransmorphConfigSettings settings)
        {
            var globalSettings = new GlobalSettings();

            _configService.BindConfigSetting(settings.AlchemyMenuEnabled,
                (SettingValueChangedArgs<bool> args) => globalSettings.AlchemyMenuEnabled = args.NewValue, true);

            _configService.BindConfigSetting(settings.CookingMenuEnabled,
                (SettingValueChangedArgs<bool> args) => globalSettings.CookingMenuEnabled = args.NewValue, true);

            return globalSettings;
        }
        public TransmogSettings ConfigureTransmogrifySettings(TransmorphConfigSettings settings)
        {
            var tmogSettings = new TransmogSettings();

            _configService.BindConfigSetting(settings.AllCharactersLearnRecipes,
                (SettingValueChangedArgs<bool> args) => tmogSettings.AllCharactersLearnRecipes = args.NewValue, true);

            return tmogSettings;
        }

        private bool MeetsMinimumVersion(string minimumVersion)
        {
            var configVersionValue = _configService.PeekSavedConfigValue(new TransmorphConfigSettings().ConfigVersion);
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
