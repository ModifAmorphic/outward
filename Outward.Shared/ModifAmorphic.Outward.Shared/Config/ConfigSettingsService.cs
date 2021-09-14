using BepInEx;
using BepInEx.Configuration;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Config.Extensions;
using ModifAmorphic.Outward.Config.Models;
using System;
using System.IO;

namespace ModifAmorphic.Outward.Config
{
    public class ConfigSettingsService
    {
#if DEBUG
        private readonly IModifLogger _logger = LoggerFactory.ConfigureLogger(DebugLoggerInfo.ModId, DebugLoggerInfo.ModName, DebugLoggerInfo.DebugLogLevel);
#endif
        public ConfigFile Config { get; }
        private readonly ConfigManagerService _configManagerService;

        public ConfigSettingsService(BaseUnityPlugin unityPlugin)
        {
            (Config, _configManagerService) = (unityPlugin.Config, new ConfigManagerService(unityPlugin));
#if DEBUG
            _logger.LogDebug($"{nameof(ConfigSettingsService)}: Bound to ConfigFile {Config.ConfigFilePath}.");
#endif
        }

        public ConfigSetting<T> BindConfigSetting<T>(ConfigSetting<T> configSetting, Action<SettingValueChangedArgs<T>> onValueChange = null, bool bindRaisesChangeEvent = false)
        {
            //This TryGetEntry is pretty much a useless check because Bind does it anyway even though
            //the documentation suggests otherwise.
            //Left it in in case it every gets changed to match the documentation.
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
            _logger.LogDebug($"Bound ConfigSetting: {configSetting.Name} to ConfigEntry {configEntry.Definition.Key} with value {configEntry.Value}");
#endif
            configSetting.SuppressValueChangedEvents = bindRaisesChangeEvent;
            return configSetting;
        }

        public bool ContainsConfigSetting<T>(ConfigSetting<T> configSetting)
        {
            return PeekConfigEntry(configSetting) != null;
        }
        /// <summary>
        /// Check for an existing setting without altering or binding it.
        /// </summary>
        /// <typeparam name="T">The type of setting to look for.</typeparam>
        /// <param name="configSetting">The setting to find.</param>
        /// <returns></returns>
        public T PeekSavedConfigValue<T>(ConfigSetting<T> configSetting)
        {
            var peekedEntry = PeekConfigEntry(configSetting);

            return peekedEntry != null ? peekedEntry.Value : default;
        }
        /// <summary>
        /// This function lets you peek at a ConfigEntry without
        /// creating the thing. This returns a temporary, unbound ConfigEntry.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configDef"></param>
        /// <returns></returns>
        private ConfigEntry<T> PeekConfigEntry<T>(ConfigSetting<T> configSetting)
        {
            //Check if the config file even exists before attempting any of this.
            //otherwise the reloads will throw exceptions because this turns off
            //saving when binding.
            if (!File.Exists(Config.ConfigFilePath))
                return null;
            //This debauchery is done because of how ConfigFile doesn't ever let you see a config value
            //until you've Bind'ed to it.  For example, to get a ConfigEntry you have to first bind to an instance of your ConfigEntry,
            //but that act of binding actually creates it (and by default saves it to file) thus making it impossible to check
            //if a setting exists or not before creating it or even get a read only copy of it.
            //TryGetEntry only will return an Bound ConfigEntries so that won't work either.
            //The only way I've found to "peek" (without reflection) is to turn off saving, bind to it to a new temporary ConfigEntry
            //with a uniquely created Default Value and check if the Value == Default Value after it's bound.  Then
            //remove the entry you added by binding so you don't actually change whatever real value it has, if any,
            //and finally reload and restore saving.

            var autosave = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet = false;

            var defaultValue = Guid.NewGuid().ToString();

            //type doesn't really matter at all unless you try to get or bind to it again as a different type.
            //They're all strings in the end...
#if DEBUG
            _logger.LogDebug($"{nameof(PeekConfigEntry)} - Peeking for ConfigEntry<string> {configSetting.Name} in section {configSetting.ToConfigDefinition().Section}." +
                $" Using DefaultValue of '{defaultValue}'.");
#endif

            if (!Config.TryGetEntry<string>(configSetting.ToConfigDefinition(), out var tempEntry))
            {
                tempEntry = Config.Bind(configSetting.ToConfigDefinition(), defaultValue, configSetting.ToConfigDescription());
            }
#if DEBUG
            _logger.LogDebug($"{nameof(PeekConfigEntry)} - Got temporary ConfigEntry<string> {tempEntry.Definition.Key} with value of '{tempEntry.Value}'");
#endif
            //Now that we have it, remove it so ConfigFile doesn't save this temporary entry
            Config.Clear();
            //Reload it to get the original value loaded into ConfigFile's internal "OrphanedEntries" list
            //as Binding to it earlier removed it.
            Config.Reload();
            //Check if it existed already or if we just created it...
            var exists = tempEntry.Value != defaultValue;
            //If it did exist then get it again typed correctly...
            ConfigEntry<T> peekedEntry = null;
            if (exists)
            {
                peekedEntry = Config.Bind(configSetting.ToConfigDefinition(), configSetting.DefaultValue, configSetting.ToConfigDescription());
#if DEBUG
                _logger.LogDebug($"{nameof(PeekConfigEntry)} - Found {configSetting.Name} in section {configSetting.ToConfigDefinition().Section}. Getting typed ConfigEntry<{typeof(T).Name}>.");
#endif
                Config.Clear();
                Config.Reload();
            }
            //Restore value
            Config.SaveOnConfigSet = autosave;

            return peekedEntry;
        }

        /// <summary>
        /// Removes the config completely. Configuration file is deleted. Config is reloaded.  Fresh start.
        /// </summary>
        public void RemoveAllSettings()
        {
            if (File.Exists(Config.ConfigFilePath))
            {
                File.WriteAllText(Config.ConfigFilePath, string.Empty);
                
                Config.Clear();
                Config.Reload();
                Config.Save();
            }
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
