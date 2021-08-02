using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents;

namespace ModifAmorphic.Outward.StashPacks
{
    public class Startup
    {
        private IModifLogger Logger => LoggerFactory.GetLogger();

        public Startup() { }
        public void Start(BaseUnityPlugin unityPlugin)
        {
            var settingsService = new SettingsService(unityPlugin, ModInfo.MinimumConfigVersion)
                                        .Configure();
            var instanceFactory = new InstanceFactory(unityPlugin, LoggerFactory.GetLogger);
            //var stashBagState = new StashBagState(instanceFactory, LoggerFactory.GetLogger);
            //var spBehaviors = new StashPackBehaviors(instanceFactory, stashBagState, LoggerFactory.GetLogger);
            var levelLoadActions = new LevelLoadingActions(instanceFactory, LoggerFactory.GetLogger);
            levelLoadActions.SubscribeToEvents();
            var bagDropActions = new BagDropActions(instanceFactory, LoggerFactory.GetLogger);
            bagDropActions.SubscribeToEvents();
            var bagPickedActions = new BagPickedActions(instanceFactory, LoggerFactory.GetLogger);
            bagPickedActions.SubscribeToEvents();
            var changedActions = new ContentsChangedActions(instanceFactory, LoggerFactory.GetLogger);
            changedActions.SubscribeToEvents();
            var saveActions = new PlayerSaveActions(instanceFactory, LoggerFactory.GetLogger);
            saveActions.SubscribeToEvents();

            ConfigurePatchLogging();
        }
        private void ConfigurePatchLogging()
        {
            EnvironmentSaveEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterSaveInstanceHolderEvents.LoggerFactory = LoggerFactory.GetLogger;
            ItemEvents.LoggerFactory = LoggerFactory.GetLogger;
            ItemContainerEvents.LoggerFactory = LoggerFactory.GetLogger;
            ItemManagerEvents.LoggerFactory = LoggerFactory.GetLogger;
            NetworkLevelLoaderEvents.LoggerFactory = LoggerFactory.GetLogger;
            SaveInstanceEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterInventoryEvents.LoggerFactory = LoggerFactory.GetLogger;
        }
    }
}
