
using BepInEx;
using HarmonyLib;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules;
using ModifAmorphic.Outward.Modules.Localization;
using ModifAmorphic.Outward.NewCaldera.AiMod;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod;
using ModifAmorphic.Outward.NewCaldera.Data;
using ModifAmorphic.Outward.NewCaldera.Services;
using ModifAmorphic.Outward.NewCaldera.Settings;
using ModifAmorphic.Outward.UnityScripts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera
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
                .AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId))
                .AddSingleton(ModifModules.GetLocalizationModule(ModInfo.ModId))
                .AddSingleton(new ModifCoroutine(services.GetService<BaseUnityPlugin>(),
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new LevelCoroutines(services.GetService<BaseUnityPlugin>(),
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new ModifGoService(services.GetService<IModifLogger>))
                .AddSingleton(new LocalizationsDirectory(CalderaSettings.PluginPath, _loggerFactory))
                .AddSingleton(new ItemLocalizationsData(services.GetService<LocalizationsDirectory>(),
                                                services.GetService<LocalizationModule>(),
                                                services.GetService<IModifLogger>))
                .AddSingleton(new BuildingStartup(services,
                                                services.GetService<ModifGoService>(),
                                                services.GetService<LevelCoroutines>(),
                                                services.GetService<ItemLocalizationsData>(),
                                                services.GetService<IModifLogger>))
                .AddSingleton(new AiStartup(services,
                                                services.GetService<ModifGoService>(),
                                                services.GetService<LevelCoroutines>(),
                                                services.GetService<ItemLocalizationsData>(),
                                                services.GetService<IModifLogger>))
                .AddSingleton(new PlayerServicesProvider(services.GetService<IModifLogger>))
                .AddSingleton(new PlayerServicesDisposer(services,
                                                services.GetService<PlayerServicesProvider>(),
                                                services.GetService<IModifLogger>));

            _loggerFactory = services.GetServiceFactory<IModifLogger>();

            //ConfigureAssetBundle();

            var scriptManager = ModifScriptsManager.Init(BepInEx.Paths.PluginPath, (UnityScripts.Logging.LogLevel)(int)Logger.LogLevel);
            NewCalderaPlugin.ModifScriptsManager = scriptManager;
            //scriptManager.AddBuildingBundle("Caldera-Foilage"
            //    , Path.Combine(CalderaSettings.PluginPath, "asset-bundles", "caldera-foilage")
            //    , Path.Combine(CalderaSettings.PluginPath, "Locales", "Caldera-Foilage"));

            //scriptManager.AddBuildingBundle("HallowedMarsh-Foilage"
            //    , Path.Combine(CalderaSettings.PluginPath, "asset-bundles", "hallowedmarsh-foilage")
            //    , Path.Combine(CalderaSettings.PluginPath, "Locales", "HallowedMarsh-Foilage"));

            scriptManager.Load();

            //DumpItemVisualAssets();
            Starter.TryStart(services.GetService<BuildingStartup>());
            Starter.TryStart(services.GetService<AiStartup>());

            
        }

        public GameObject ConfigureAssetBundle()
        {
            const string bundleName = "modifamorphic-buildings";
            var scriptsGo = AttachScripts(typeof(BuildingBinder).Assembly);
            Logger.LogDebug($"Loading asset bundle {bundleName}.");
            var buildingsBundle = LoadAssetBundle(_services.GetService<BaseUnityPlugin>().GetPluginDirectory(), "asset-bundles", bundleName);
            var allAssetNames = buildingsBundle.GetAllAssetNames();
            var logAssets = "";
            foreach (string name in allAssetNames)
            {
                logAssets += name + ", ";
            }
            Logger.LogDebug($"Assets in Bundle: " + logAssets);


            var buildingsPrefab = buildingsBundle.LoadAsset<GameObject>("assets/prefabs/buildings/items/0000000_building_3Phase.prefab");
            Logger.LogDebug($"Loaded assets/prefabs/buildings/items/0000000_building_3Phase.prefab.");
            if (buildingsPrefab.activeSelf)
            {
                Logger.LogDebug($"Deactivating 0000000_building_3Phase prefab.");
                buildingsPrefab.SetActive(false);
            }
            NewCalderaPlugin.BuildingItemPrefab = buildingsPrefab;
            UnityEngine.Object.DontDestroyOnLoad(buildingsPrefab);

            var mediumBuildingShell = buildingsBundle.LoadAsset<GameObject>("assets/prefabs/buildings/visuals/mediumbuildingshell.prefab");
            Logger.LogDebug($"Loaded assets/prefabs/buildings/visuals/mediumbuildingshell.prefab.");
            if (mediumBuildingShell.activeSelf)
            {
                Logger.LogDebug($"Deactivating mediumbuildingshell prefab.");
                mediumBuildingShell.SetActive(false);
            }
            NewCalderaPlugin.MediumBuildingShellPrefab = mediumBuildingShell;
            UnityEngine.Object.DontDestroyOnLoad(mediumBuildingShell);


            var tinyBuildingShell = buildingsBundle.LoadAsset<GameObject>("assets/prefabs/buildings/visuals/TinyBuildingShell_2Phase.prefab");
            Logger.LogDebug($"Loaded assets/prefabs/buildings/visuals/tinybuildingshell.prefab.");
            if (tinyBuildingShell.activeSelf)
            {
                Logger.LogDebug($"Deactivating tinybuildingshell prefab.");
                tinyBuildingShell.SetActive(false);
            }
            NewCalderaPlugin.TinyBuildingShellPrefab = tinyBuildingShell;
            UnityEngine.Object.DontDestroyOnLoad(tinyBuildingShell);

            buildingsBundle.Unload(false);

            //var modGo = _services.GetService<ModifGoService>()
            //                     .GetModResources(ModInfo.ModId, false);

            //buildingsPrefab.transform.SetParent(modGo.transform);

            //var pspPrefab = buildingsPrefab.transform.Find("PlayersServicesProvider").gameObject;
            //var modActiveGo = _services.GetService<ModifGoService>()
            //                     .GetModResources(ModInfo.ModId, true);
            //var psp = UnityEngine.Object.Instantiate(pspPrefab);

            //psp.transform.SetParent(modActiveGo.transform);
            //psp.name = "PlayersServicesProvider";

            //var positionBgPrefab = actionUiPrefab.transform.Find("PositionableBg").gameObject;
            //var positionBg = UnityEngine.Object.Instantiate(positionBgPrefab, modGo.transform);
            //positionBg.name = "PositionableBg";

            UnityEngine.Object.Destroy(scriptsGo);

            return buildingsPrefab;

        }
        public GameObject AttachScripts(Assembly sourceAssembly)
        {
            var monoBehaviours = sourceAssembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)) && !type.GetTypeInfo().IsAbstract);
            var goSvc = _services.GetService<ModifGoService>();
            var modGo = goSvc.GetModResources(ModInfo.ModId, false);
            var scriptGo = new GameObject("scripts");
            scriptGo.transform.SetParent(modGo.transform);

            foreach (var t in monoBehaviours)
            {
#if DEBUG
                Logger.LogTrace($"Attaching script {t.FullName}");
#endif
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

        private void DumpItemVisualAssets()
        {
            var item_visuals_bundle = LoadAssetBundle(@"D:\Games\SteamLibrary\steamapps\common\Outward\Outward_Defed\Outward Definitive Edition_Data\", "StreamingAssets", "item_visuals");
            var allAssetNames = item_visuals_bundle.GetAllAssetNames();
            var logAssets = "";
            foreach (string name in allAssetNames)
            {
                logAssets += "\n\t" + name;
            }
            Logger.LogDebug($"Assets in item_visuals Bundle: " + logAssets);

            item_visuals_bundle.Unload(false);
        }
    }
}
