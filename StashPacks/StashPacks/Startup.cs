using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections.Generic;
using System.Text;

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
            var spBehaviors = new StashPackBehaviors(instanceFactory, LoggerFactory.GetLogger);
            ConfigurePatchLogging();
        }
        private void ConfigurePatchLogging()
        {
            EnvironmentSaveEvents.LoggerFactory = LoggerFactory.GetLogger;
            CharacterSaveInstanceHolderEvents.LoggerFactory = LoggerFactory.GetLogger;
        }
    }
}
