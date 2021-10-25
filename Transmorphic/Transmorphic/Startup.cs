
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules;
using ModifAmorphic.Outward.Transmorphic.Settings;
using ModifAmorphic.Outward.Transmorphic.Transmog;
using ModifAmorphic.Outward.Transmorphic.Cooking;
using ModifAmorphic.Outward.Transmorphic.Alchemy;
using BepInEx;
using System;

namespace ModifAmorphic.Outward.Transmorphic
{
    internal class Startup
    {
        private ServicesProvider _services;
        private Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        internal void Start(ServicesProvider services)
        {
            _services = services;
            var settingsService = new SettingsService(services.GetService<BaseUnityPlugin>(), ModInfo.MinimumConfigVersion);
            var confSettings = settingsService.ConfigureSettings();

            services
                .AddSingleton(confSettings)
                .AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId))
                .AddSingleton(ModifModules.GetPreFabricatorModule(ModInfo.ModId))
                .AddSingleton(ModifModules.GetItemVisualizerModule(ModInfo.ModId))
                .AddSingleton(ModifModules.GetCustomCraftingModule(ModInfo.ModId))
                .AddSingleton(new LevelCoroutines(services.GetService<BaseUnityPlugin>(), 
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new CookingStartup(services, settingsService, confSettings))
                .AddSingleton(new AlchemyStartup(services, settingsService, confSettings))
                .AddSingleton(new TransmogStartup(services, settingsService, confSettings));

            _loggerFactory = services.GetServiceFactory<IModifLogger>();

            TryStartMenu(services.GetService<CookingStartup>());
            TryStartMenu(services.GetService<AlchemyStartup>());
            TryStartMenu(services.GetService<TransmogStartup>());

        }
        private void TryStartMenu(IStartable menuStartup)
        {
            try
            {
                menuStartup.Start();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start menu {menuStartup?.GetType()}.", ex);
            }
        }
    }
}
