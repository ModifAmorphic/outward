using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Network;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks
{
    public class Startup
    {
        private IModifLogger Logger => LoggerFactory.GetLogger();

        public Startup() { }
        public void Start(BaseUnityPlugin unityPlugin)
        {
            var settingsService = new SettingsService(unityPlugin, ModInfo.MinimumConfigVersion);
            var configSettings = settingsService.ConfigureSettings();
            var stashPackNet = ConfigureStashPackNet();
            BagStateService.ConfigureNet(stashPackNet);
            var hostSettings = settingsService.ConfigureHostSettings(configSettings, stashPackNet);
            var instanceFactory = new InstanceFactory(unityPlugin, configSettings, hostSettings, stashPackNet, LoggerFactory.GetLogger);
            var actionInstances = new ActionInstanceManager(instanceFactory, LoggerFactory.GetLogger);
            actionInstances.StartActions();

            ConfigurePatchLogging();
        }
        private StashPackNet ConfigureStashPackNet()
        {
            var gameObject = new GameObject(nameof(StashPackNet))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var stashPackNet = gameObject.AddComponent<StashPackNet>();
            stashPackNet.LoggerFactory = LoggerFactory.GetLogger;

            return stashPackNet;
        }
        private void ConfigurePatchLogging()
        {
            LobbySystemEvents.LoggerFactory = LoggerFactory.GetLogger;
            ConnectPhotonMasterEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterSaveInstanceHolderEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterEvents.LoggerFactory = LoggerFactory.GetLogger;
            ItemContainerEvents.LoggerFactory = LoggerFactory.GetLogger;
            ItemManagerEvents.LoggerFactory = LoggerFactory.GetLogger;
            NetworkLevelLoaderEvents.LoggerFactory = LoggerFactory.GetLogger;
            SaveInstanceEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterInventoryEvents.LoggerFactory = LoggerFactory.GetLogger;
            ItemEvents.LoggerFactory = LoggerFactory.GetLogger;
            BagEvents.LoggerFactory = LoggerFactory.GetLogger;
            InteractionDisplayEvents.LoggerFactory = LoggerFactory.GetLogger;
        }
    }
}
