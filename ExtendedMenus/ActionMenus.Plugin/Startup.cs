﻿
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.ActionMenus.Settings;
using BepInEx;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System.Linq;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.ActionMenus.Services;
using ModifAmorphic.Outward.ActionMenus.Plugin.Services;
using Rewired;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;

namespace ModifAmorphic.Outward.ActionMenus
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
                .AddSingleton(settingsService.ConfigureHotbarSettings(confSettings))
                .AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId))
                .AddSingleton(new ModifCoroutine(services.GetService<BaseUnityPlugin>(),
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new LevelCoroutines(services.GetService<BaseUnityPlugin>(),
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new ModifGoService(services.GetService<IModifLogger>))
                .AddSingleton(new RewiredListener(services.GetService<BaseUnityPlugin>(),
                                                   services.GetService<LevelCoroutines>(),
                                                   confSettings,
                                                   services.GetService<IModifLogger>));
            

            _loggerFactory = services.GetServiceFactory<IModifLogger>();

            var menuOverhaul = ConfigureAssetBundle();

            services
                .AddSingleton(new PlayerMenuService(services.GetService<BaseUnityPlugin>(),
                                                  menuOverhaul.GetComponentInChildren<PlayerMenu>(true).gameObject,
                                                  services.GetService<LevelCoroutines>(),
                                                  services.GetService<ModifGoService>(),
                                                  services.GetService<HotbarSettings>(),
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new HotbarServicesInjector(services,
                                                  services.GetService<ModifGoService>(),
                                                  services.GetService<LevelCoroutines>(),
                                                  services.GetService<HotbarSettings>(),
                                                  services.GetService<IModifLogger>));
        }
        public GameObject ConfigureAssetBundle()
        {
            var scriptsGo = AttachScripts(typeof(ActionSlot).Assembly);
            Logger.LogDebug($"Loading asset bundle action-menus-overhaul.");
            var menuBundle = LoadAssetBundle(_services.GetService<BaseUnityPlugin>().GetPluginDirectory(), "asset-bundles", "action-menus-overhaul");
            var allAssetNames = menuBundle.GetAllAssetNames();
            var logAssets = "";
            foreach (string name in allAssetNames)
            {
                logAssets += name + ", ";
            }
            Logger.LogDebug($"Assets in Bundle: " + logAssets);
            var menuOverhaulPrefab = menuBundle.LoadAsset<GameObject>("assets/prefabs/menuoverhaul.prefab");
            menuOverhaulPrefab.SetActive(false);
            Logger.LogDebug($"Loaded asset assets/prefabs/menuoverhaul.prefab.");
            UnityEngine.Object.DontDestroyOnLoad(menuOverhaulPrefab);
            menuBundle.Unload(false);

            var modGo = _services.GetService<ModifGoService>()
                                 .GetModResources(ModInfo.ModId, false);

            menuOverhaulPrefab.transform.SetParent(modGo.transform);

            var pspPrefab = menuOverhaulPrefab.transform.Find("PlayersServicesProvider").gameObject;
            var modActiveGo = _services.GetService<ModifGoService>()
                                 .GetModResources(ModInfo.ModId, true);
            var psp = UnityEngine.Object.Instantiate(pspPrefab);

            psp.transform.SetParent(modActiveGo.transform);
            psp.name = "PlayersServicesProvider";

            UnityEngine.Object.Destroy(pspPrefab);
            UnityEngine.Object.Destroy(scriptsGo);

            return menuOverhaulPrefab;

        }
        public GameObject AttachScripts(Assembly sourceAssembly)
        {
            var scriptComponentTypes = sourceAssembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(UnityScriptComponentAttribute), true).Length > 0);
            var goSvc = _services.GetService<ModifGoService>();
            var modGo = goSvc.GetModResources(ModInfo.ModId, false);
            var scriptGo = new GameObject("scripts");
            scriptGo.transform.SetParent(modGo.transform);

            foreach (var t in scriptComponentTypes)
            {
                scriptGo.AddComponent(t);
            }
            return scriptGo;
        }

        private AssetBundle LoadAssetBundle(string pluginPath, string bundleDirectory, string bundleFileName)
        {
            using (var assetStream = new FileStream
                    (
                        Path.Combine(pluginPath, Path.Combine(bundleDirectory, bundleFileName)
                    ), FileMode.Open, FileAccess.Read)
                  )
            {
                return AssetBundle.LoadFromStream(assetStream);
            }
        }
    }
}
