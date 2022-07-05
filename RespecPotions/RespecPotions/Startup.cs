using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules;
using ModifAmorphic.Outward.Modules.CharacterMods;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Modules.Merchants;
using ModifAmorphic.Outward.RespecPotions.Settings;
using UnityEngine.SceneManagement;

namespace ModifAmorphic.Outward.RespecPotions
{
    public class Startup
    {
        public static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);
        public Startup() { }
        public void Start(ServicesProvider services)
        {
            var settingsService = new SettingsService(services.GetService<BaseUnityPlugin>(), ModInfo.MinimumConfigVersion);
            services.AddSingleton(settingsService.ConfigureSettings());
            services.AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId));

            services.AddFactory(() => ResourcesPrefabManager.Instance);
            services.AddSingleton(ModifModules.GetCharacterInstancesModule(ModInfo.ModId));
            services.AddSingleton(ModifModules.GetPreFabricatorModule(ModInfo.ModId));
            services.AddSingleton(ModifModules.GetMerchantModule(ModInfo.ModId));

            services.AddFactory(() =>
                new SchoolService(services));
            services.AddSingleton(
                new PotionItemService(
                        services.GetService<CharacterInstances>(),
                        services.GetService<ResourcesPrefabManager>(),
                        services.GetService<PreFabricator>(),
                        services.GetService<MerchantModule>(),
                        services.GetService<BaseUnityPlugin>(),
                        services.GetService<RespecConfigSettings>(),
                        services.GetService<IModifLogger>
                    ));;
            SceneManager.sceneLoaded += (s, l) =>
            {
                services.GetService<PotionItemService>().StartCreatePotionItemsCoroutine();
            };
        }
        
    }
}
