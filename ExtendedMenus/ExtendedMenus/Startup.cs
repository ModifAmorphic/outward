
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.ExtendedMenus.Settings;
using BepInEx;
using System;
using System.IO;
using UnityEngine;

namespace ModifAmorphic.Outward.ExtendedMenus
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
                .AddSingleton(new LevelCoroutines(services.GetService<BaseUnityPlugin>(),
                                                  services.GetService<IModifLogger>));

            _loggerFactory = services.GetServiceFactory<IModifLogger>();

            var menuAssets = LoadAssetMenu(Path.GetDirectoryName(services.GetService<BaseUnityPlugin>().Info.Location));
            var utilityMenu = menuAssets.LoadAsset("UtilityMenu");
            GameObject.Instantiate(utilityMenu);
            menuAssets.Unload(false);
        }
        private AssetBundle LoadAssetMenu(string pluginPath)
        {
            using (var assetStream = new FileStream(Path.Combine(
                    pluginPath, Path.Combine("assets", "utilitymenues"))
                , FileMode.Open, FileAccess.Read))
            {
                return AssetBundle.LoadFromStream(assetStream);
            }

        }

    }
}
