
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Settings;
using BepInEx;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System.Linq;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.UI.Services;
using ModifAmorphic.Outward.UI.Plugin.Services;
using Rewired;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using HarmonyLib;
using ModifAmorphic.Outward.UI.Patches;

namespace ModifAmorphic.Outward.UI
{
    internal class Startup
    {
        private ServicesProvider _services;
        private Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        internal void Start(Harmony harmony, ServicesProvider services)
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

            var actionUIPrefab = ConfigureAssetBundle();

            services
                .AddSingleton(new SharedServicesInjector(
                    services, services.GetService<IModifLogger>))
                .AddSingleton(new PositionsService(
                        services.GetService<LevelCoroutines>(),
                        services.GetService<ModifGoService>(),
                        services.GetService<IModifLogger>))
                .AddSingleton(new PlayerMenuService(services.GetService<BaseUnityPlugin>(),
                                                  actionUIPrefab.GetComponentInChildren<PlayerActionMenus>(true).gameObject,
                                                  services.GetService<PositionsService>(),
                                                  services.GetService<LevelCoroutines>(),
                                                  services.GetService<ModifGoService>(),
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new PositionsServicesInjector(_services,
                                services.GetService<ModifGoService>(),
                                services.GetService<LevelCoroutines>(),
                                services.GetService<IModifLogger>));


            SplitPlayerPatches.SetCharacterAfter += AddSharedServices;

            services
                .AddSingleton(new HotbarsStartup(
                    services
                    , services.GetService<ModifGoService>()
                    , services.GetService<LevelCoroutines>()
                    , services.GetService<HotbarSettings>()
                    , _loggerFactory))
                .AddSingleton(new DurabilityDisplayStartup(
                    services
                    , services.GetService<ModifGoService>()
                    , services.GetService<LevelCoroutines>()
                    , _loggerFactory));

            services.GetService<HotbarsStartup>().Start();
            services.GetService<DurabilityDisplayStartup>().Start();
        }
        public GameObject ConfigureAssetBundle()
        {
            var scriptsGo = AttachScripts(typeof(PlayerActionMenus).Assembly);
            Logger.LogDebug($"Loading asset bundle action-ui.");
            var menuBundle = LoadAssetBundle(_services.GetService<BaseUnityPlugin>().GetPluginDirectory(), "asset-bundles", "action-ui");
            var allAssetNames = menuBundle.GetAllAssetNames();
            var logAssets = "";
            foreach (string name in allAssetNames)
            {
                logAssets += name + ", ";
            }
            Logger.LogDebug($"Assets in Bundle: " + logAssets);
            var actionUiPrefab = menuBundle.LoadAsset<GameObject>("assets/prefabs/actionui.prefab");
            actionUiPrefab.SetActive(false);
            Logger.LogDebug($"Loaded asset assets/prefabs/actionui.prefab.");
            UnityEngine.Object.DontDestroyOnLoad(actionUiPrefab);

            //var positionBgPrefab = menuBundle.LoadAsset<GameObject>("assets/prefabs/positionablebg.prefab");
            //positionBgPrefab.SetActive(false);
            //Logger.LogDebug($"Loaded asset assets/prefabs/positionablebg.prefab.");
            //UnityEngine.Object.DontDestroyOnLoad(positionBgPrefab);

            menuBundle.Unload(false);

            var modGo = _services.GetService<ModifGoService>()
                                 .GetModResources(ModInfo.ModId, false);

            actionUiPrefab.transform.SetParent(modGo.transform);

            var pspPrefab = actionUiPrefab.transform.Find("PlayersServicesProvider").gameObject;
            var modActiveGo = _services.GetService<ModifGoService>()
                                 .GetModResources(ModInfo.ModId, true);
            var psp = UnityEngine.Object.Instantiate(pspPrefab);

            psp.transform.SetParent(modActiveGo.transform);
            psp.name = "PlayersServicesProvider";

            var positionBgPrefab = actionUiPrefab.transform.Find("PositionableBg").gameObject;
            var positionBg = UnityEngine.Object.Instantiate(positionBgPrefab, modGo.transform);
            positionBg.name = "PositionableBg";

            var actionSpritesPrefab = actionUiPrefab.transform.Find("ActionSprites").gameObject;
            var actionSprites = UnityEngine.Object.Instantiate(actionSpritesPrefab);

            actionSprites.transform.SetParent(modActiveGo.transform);
            actionSprites.name = "ActionSprites";

            UnityEngine.Object.Destroy(pspPrefab);
            UnityEngine.Object.Destroy(positionBgPrefab);
            UnityEngine.Object.Destroy(actionSpritesPrefab);
            UnityEngine.Object.Destroy(scriptsGo);

            return actionUiPrefab;

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
                Logger.LogTrace($"Attaching script {t.FullName}");
                scriptGo.AddComponent(t);
            }

            //Logger.LogDebug($"Attached component scripts at {scriptGo.GetPath()}.");

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

        private void AddSharedServices(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            psp.AddSingleton(new ProfileManager(splitPlayer.RewiredID));
        }
    }
}
