using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Shared.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward
{
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, ModVersion)]
    public class SharedPlugin : BaseUnityPlugin
    {
        public const string ModId = "modifamorphic.outward.shared";
        public const string ModName = "ModifAmorphic Shared Library";
        public const string ModVersion = "0.1.0";

        private Logger _logger;

        internal void Awake()
        {
            var harmony = new Harmony(ModId);
            try
            {
                LoggerEvents.LoggerLoaded += LoggerEvents_LoggerLoaded;
                ConfigureLogger();
                _logger.LogDebug($"[{ModName}] Registering Event Subscriptions");
                EventSubscriberService.RegisterSubscriptions(_logger);
                //raise logger event again because nothing else was subscribed the first time.
                LoggerEvents.RaiseLoggerConfigured(this, _logger);

                _logger.LogDebug($"[{ModName}] Patching");
                //KeyBindings.Listeners.LocalCharacterControlPatches.Patch();

                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[{ModName}] Failed to enable {ModId} {ModName}. Exception: \\n{ex}");
                harmony.UnpatchSelf();
                throw;
            }
        }

        private void LoggerEvents_LoggerLoaded(object sender, Logger e)
        {
            _logger = e;
        }

        private void ConfigureLogger()
        {
            var logLevelDef = new ConfigDefinition("", "logLevel");
            var logLevelDesc = new ConfigDescription("Minimum log level.  Info is default.", null, new ConfigurationManagerAttributes { IsAdvanced = true });

            var logLevelEntry = GetConfiguredLogLevel(logLevelDef, logLevelDesc);

            SubscribeToLogLevelChanges(logLevelEntry);

            LoggerEvents.RaiseLoggerConfigured(this, InternalLoggerFactory.GetLogger(logLevelEntry.Value, ModName));
        }
        private ConfigEntry<LogLevel> GetConfiguredLogLevel(ConfigDefinition logLevelDef, ConfigDescription logLevelDesc)
        {
            if (!this.Config.TryGetEntry<LogLevel>(logLevelDef, out var logLevelEntry))
            {
                logLevelEntry = this.Config.Bind<LogLevel>(logLevelDef, LogLevel.Info, logLevelDesc);
            }
            return logLevelEntry;
        }
        private void SubscribeToLogLevelChanges(ConfigEntry<LogLevel> logLevelEntry)
        {
            logLevelEntry.SettingChanged += ((object sender, EventArgs e) => {
                var changedLogLevelEntry = GetConfiguredLogLevel(logLevelEntry.Definition, logLevelEntry.Description);
                LoggerEvents.RaiseLoggerConfigured(this, InternalLoggerFactory.GetLogger(changedLogLevelEntry.Value, ModName));
            });
        }
    }
}
