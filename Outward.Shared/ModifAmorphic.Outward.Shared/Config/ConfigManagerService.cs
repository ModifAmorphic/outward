using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Shared.Config
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
                throw new MissingMemberException(typeof(BaseUnityPlugin).Name, "ConfigurationManager.ConfigurationManager");
            }
        }

        public void RefreshConfigManager()
        {
            var buildSettingList = _reflectedConfigManager.GetType().GetMethod("BuildSettingList");
#if DEBUG
            (new Logger(LogLevel.Debug, ModInfo.ModName)).LogDebug(
                $"[{ModInfo.ModName}] - Refreshing ConfigurationManager " +
                $"Reflected ConfigurationManager found? {_reflectedConfigManager != null}. " +
                $"BuildSettingList method found? {buildSettingList != null}.");
#endif
            if (buildSettingList != null)
                buildSettingList.Invoke(_reflectedConfigManager, null);
        }


    }
}
