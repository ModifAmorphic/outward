using BepInEx;
using BepInEx.Configuration;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Shared.Config.Extensions;
using ModifAmorphic.Outward.Shared.Config.Models;
using System;

namespace ModifAmorphic.Outward.Shared.Config
{
    public class ConfigService
    {
#if DEBUG
        private readonly Logger _logger = new Logger(LogLevel.Trace, ModInfo.ModName);
#endif
        public ConfigFile Config { get; }
        private readonly ConfigManagerService _configManagerService;

        public ConfigService(BaseUnityPlugin unityPlugin) => (Config, _configManagerService) = (unityPlugin.Config, new ConfigManagerService(unityPlugin));

        public ConfigSetting<T> BindConfigSetting<T>(ConfigSetting<T> configSetting, Action<SettingValueChangedArgs<T>> onValueChange = null, bool bindRaisesChangeEvent = false)
        {
            if (!Config.TryGetEntry<T>(configSetting.ToConfigDefinition(), out var configEntry))
            {
                configEntry = Config.Bind(configSetting.ToConfigDefinition(), configSetting.DefaultValue, configSetting.ToConfigDescription());
            }
            configSetting.SuppressValueChangedEvents = !bindRaisesChangeEvent;
            configSetting.BoundConfigEntry = configEntry;
            configSetting.IsVisible = configEntry.Description.ConfigurationManagerAttributes().Browsable ?? true;
            configSetting.ValueChanged += (object sender, SettingValueChangedArgs<T> e) => onValueChange?.Invoke(e);
            configEntry.SettingChanged += (object sender, EventArgs e) => configSetting.Value = configSetting.BoundConfigEntry.Value;
            configSetting.Value = configSetting.BoundConfigEntry.Value;
#if DEBUG
            _logger.LogDebug($"[{ModInfo.ModName}] - Bound ConfigSetting: {configSetting.Name} to ConfigEntry {configEntry.Definition.Key} with value {configEntry.Value}");
#endif
            configSetting.SuppressValueChangedEvents = bindRaisesChangeEvent;
            return configSetting;
        }
        /// <summary>
        /// If the setting is currently visible, hides the setting and refreshes the configuration manager.  If the setting is already hidden, no action is taken.
        /// </summary>
        /// <typeparam name="T">The type of ConfigSetting</typeparam>
        /// <param name="configSetting">The ConfigSetting to hide.</param>
        public void HideSettingAndRefresh<T>(ConfigSetting<T> configSetting)
        {
            if (configSetting.IsVisible || (configSetting.BoundConfigEntry.Description.ConfigurationManagerAttributes().Browsable ?? true))
            {
                configSetting.Hide();
                _configManagerService.RefreshConfigManager();
            }
        }
        /// <summary>
        /// If the setting is currently hidden, shows the setting and refreshes the configuration manager.  If the setting is already shown, no action is taken.
        /// </summary>
        /// <typeparam name="T">The type of ConfigSetting</typeparam>
        /// <param name="configSetting">The ConfigSetting to show.</param>
        public void ShowSettingAndRefresh<T>(ConfigSetting<T> configSetting)
        {
            if (!configSetting.IsVisible || !(configSetting.BoundConfigEntry.Description.ConfigurationManagerAttributes().Browsable ?? true))
            {
                configSetting.Show();
                _configManagerService.RefreshConfigManager();
            }
        }
    }
}
