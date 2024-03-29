﻿using BepInEx;
using ModifAmorphic.Outward.Config;
using ModifAmorphic.Outward.Config.Extensions;
using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.RespecPotions.Settings
{
    class SettingsService
    {
        private readonly ConfigManagerService _configManagerService;
        private readonly ConfigSettingsService _configService;
        private readonly string _minConfigVersion;

#if DEBUG
        private readonly Logger _logger = new Logger(LogLevel.Trace, ModInfo.ModName);
#endif
        public SettingsService(BaseUnityPlugin plugin, string minConfigVersion)
        {
            (_configManagerService, _configService, _minConfigVersion) = (new ConfigManagerService(plugin), new ConfigSettingsService(plugin), minConfigVersion);
        }

        public RespecConfigSettings ConfigureSettings()
        {
            var settings = new RespecConfigSettings();

            if (!MeetsMinimumVersion(_minConfigVersion))
            {
#if DEBUG
                _logger.LogInfo($"Minimum version requirement not met. Minumum version is {_minConfigVersion}. Resetting all settings.");
#endif
                _configService.RemoveAllSettings();
            }

            #region Main Section
            //_configService.BindConfigSetting(settings.ConfigSetting, null);

            #endregion

            #region Hidden Section
            _configService.BindConfigSetting(settings.PotionValue, null)
                .Hide();
            _configService.BindConfigSetting(settings.ShopMinAmount, null)
                .Hide();
            _configService.BindConfigSetting(settings.ShopMaxAmount, null)
                .Hide();
            #endregion

            #region Advanced Section
            //Logging Level
            _configService.BindConfigSetting(settings.LogLevel,
                (SettingValueChangedArgs<LogLevel> args) => LoggerFactory.ConfigureLogger(ModInfo.ModId, ModInfo.ModName, settings.LogLevel.Value)
                , true);
            //The Version the config was originally created with
            _configService.BindConfigSetting(settings.ConfigVersion, null);

            #endregion

            return settings;
        }
        private bool MeetsMinimumVersion(string minimumVersion)
        {
            var configVersionValue = _configService.PeekSavedConfigValue(new RespecConfigSettings().ConfigVersion);
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
