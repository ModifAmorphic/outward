using BepInEx;
using ModifAmorphic.Outward.Config;
using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.NewCaldera.Settings
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
        //public HotbarSettings ConfigureHotbarSettings(ConfigSettings settings)
        //{
        //    var hotbarSettings = new HotbarSettings();

        //    return hotbarSettings;
        //}

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
