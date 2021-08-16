using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions;

namespace ModifAmorphic.Outward.StashPacks
{
    public class Startup
    {
        private IModifLogger Logger => LoggerFactory.GetLogger();

        public Startup() { }
        public void Start(BaseUnityPlugin unityPlugin)
        {
            var settings = new SettingsService(unityPlugin, ModInfo.MinimumConfigVersion)
                                        .Configure();
            var instanceFactory = new InstanceFactory(unityPlugin, settings, LoggerFactory.GetLogger);
            var actionInstances = new ActionInstanceManager(instanceFactory, LoggerFactory.GetLogger);
            actionInstances.StartActions();

            ConfigurePatchLogging();
        }
        private void ConfigurePatchLogging()
        {
            EnvironmentSaveEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterSaveInstanceHolderEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterEvents.LoggerFactory = LoggerFactory.GetLogger;
            ItemContainerEvents.LoggerFactory = LoggerFactory.GetLogger;
            ItemManagerEvents.LoggerFactory = LoggerFactory.GetLogger;
            NetworkLevelLoaderEvents.LoggerFactory = LoggerFactory.GetLogger;
            SaveInstanceEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterInventoryEvents.LoggerFactory = LoggerFactory.GetLogger;
        }
    }
}
