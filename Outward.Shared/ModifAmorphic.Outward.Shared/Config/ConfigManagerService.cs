using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Config
{
    public class ConfigManagerService
    {
        //private readonly ConfigService _configService;
        private readonly BaseUnityPlugin _unityPlugin;
        private readonly object _reflectedConfigManager;
        public ConfigManagerService(BaseUnityPlugin unityPlugin)
        {
            _unityPlugin = unityPlugin;
            _reflectedConfigManager = _unityPlugin.GetComponent("ConfigurationManager.ConfigurationManager");
            if (_reflectedConfigManager == null)
            {
#if DEBUG
                var _logger = LoggerFactory.ConfigureLogger(DefaultLoggerInfo.ModId, DefaultLoggerInfo.ModName, DefaultLoggerInfo.DebugLogLevel);
                _logger.LogWarning("ConfigurationManager.ConfigurationManager Component not found. Refreshing will be disabled.\n" + new MissingMemberException(typeof(BaseUnityPlugin).Name, "ConfigurationManager.ConfigurationManager"));
#endif
            }
        }

        public void RefreshConfigManager()
        {
            if (_reflectedConfigManager == null)
                return;

            var buildSettingList = _reflectedConfigManager.GetType().GetMethod("BuildSettingList");
#if DEBUG
            var _logger = LoggerFactory.ConfigureLogger(DefaultLoggerInfo.ModId, DefaultLoggerInfo.ModName, DefaultLoggerInfo.DebugLogLevel);
            _logger.LogDebug(
                $"Refreshing ConfigurationManager " +
                $"Reflected ConfigurationManager found? {_reflectedConfigManager != null}. " +
                $"BuildSettingList method found? {buildSettingList != null}.");
#endif
            if (buildSettingList != null)
                buildSettingList.Invoke(_reflectedConfigManager, null);
        }


    }
}
