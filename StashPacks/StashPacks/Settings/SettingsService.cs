using BepInEx;
using ModifAmorphic.Outward.Config;
using ModifAmorphic.Outward.Config.Models;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Network;
using System;

namespace ModifAmorphic.Outward.StashPacks.Settings
{
    class SettingsService
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

        public StashPacksConfigSettings ConfigureSettings()
        {
            var settings = new StashPacksConfigSettings();

            if (!MeetsMinimumVersion(_minConfigVersion))
            {
#if DEBUG
                _logger.LogInfo($"Minimum version requirement not met. Minumum version is {_minConfigVersion}. Resetting all settings.");
#endif
                _configService.RemoveAllSettings();
            }

            #region Main Section
            //Prefer pick up stashpack to pouch over bag
            _configService.BindConfigSetting(settings.PreferPickupToPouch, null);

            //Allow crafting from items in stash packs
            _configService.BindConfigSetting(settings.CraftingFromStashPackItems, null);

            //Allow use in all scenes
            _configService.BindConfigSetting(settings.AllScenesEnabled, null);

            _configService.BindConfigSetting(settings.DisableBagScalingRotation, null);
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
        public StashPackHostSettings ConfigureHostSettings(StashPacksConfigSettings configSettings, StashPackNet stashPackNet)
        {
            var hostSettings = new StashPackHostSettings()
            {
                AllScenesEnabled = configSettings.AllScenesEnabled.Value,
                DisableBagScalingRotation = configSettings.DisableBagScalingRotation.Value
            };

            configSettings.AllScenesEnabled.ValueChanged += (s, v) =>
            {
                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    hostSettings.AllScenesEnabled = v.NewValue;
                    stashPackNet.SendHostSettings(hostSettings);
                }
            };
            configSettings.DisableBagScalingRotation.ValueChanged += (s, v) =>
            {
                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    hostSettings.DisableBagScalingRotation = v.NewValue;
                    stashPackNet.SendHostSettings(hostSettings);
                }
            };
            stashPackNet.SendHostSettings(hostSettings);
            return hostSettings;
        }
        private bool MeetsMinimumVersion(string minimumVersion)
        {
            var configVersionValue = _configService.PeekSavedConfigValue(new StashPacksConfigSettings().ConfigVersion);
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
