using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Network;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions;
using System;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks
{
    public class Startup
    {
        public Startup() { }
        public void Start(ServicesProvider services)
        {
            var settingsService = new SettingsService(services.GetService<BaseUnityPlugin>(), ModInfo.MinimumConfigVersion);
            services.AddSingleton(settingsService.ConfigureSettings());
            services.AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId));

            Internal.ManifestFiles.RemoveOtherVersions(services.GetService<IModifLogger>());

            services.ConfigureStashPackNet();
            BagStateService.ConfigureNet(services.GetService<StashPackNet>());

            services.AddSingleton(
                settingsService.ConfigureHostSettings(
                    services.GetService<StashPacksConfigSettings>(), 
                    services.GetService<StashPackNet>())
                );

            services.AddSingleton(new PlayerCoroutines(
                services.GetService<BaseUnityPlugin>(),
                services.GetServiceFactory<IModifLogger>()));
            services.AddSingleton(new LevelCoroutines(
                services.GetService<BaseUnityPlugin>(),
                services.GetServiceFactory<IModifLogger>()));
            services.AddSingleton(new ItemCoroutines(
                services.GetService<BaseUnityPlugin>(),
                () => ItemManager.Instance,
                services.GetServiceFactory<IModifLogger>()));

            var instanceFactory = new InstanceFactory(services);

            var actionInstances = new ActionInstanceManager(instanceFactory, services.GetService<IModifLogger>);
            actionInstances.StartActions();

            ConfigurePatchLogging(services.GetService<IModifLogger>);
        }
        private void ConfigurePatchLogging(Func<IModifLogger> loggerFactory)
        {
            LobbySystemEvents.LoggerFactory = loggerFactory;
            ConnectPhotonMasterEvents.LoggerFactory = loggerFactory;
            CharacterSaveInstanceHolderEvents.LoggerFactory = loggerFactory;
            CharacterEvents.LoggerFactory = loggerFactory;
            ItemContainerEvents.LoggerFactory = loggerFactory;
            ItemManagerEvents.LoggerFactory = loggerFactory;
            NetworkLevelLoaderEvents.LoggerFactory = loggerFactory;
            SaveInstanceEvents.LoggerFactory = loggerFactory;
            CharacterInventoryEvents.LoggerFactory = loggerFactory;
            ItemEvents.LoggerFactory = loggerFactory;
            BagEvents.LoggerFactory = loggerFactory;
            InteractionDisplayEvents.LoggerFactory = loggerFactory;
        }
    }
}
