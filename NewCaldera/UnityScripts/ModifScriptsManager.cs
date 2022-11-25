using ModifAmorphic.Outward.UnityScripts.Models;
using ModifAmorphic.Outward.UnityScripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class ModifScriptsManager : MonoBehaviour
    {
        private static bool _isAttached;
        private static ModifScriptsManager _instance;
        public static ModifScriptsManager Instance => _instance;

        public Logging.Logger Logger { get; private set; }

        public ModifGoService ModifGoService { get; private set; }

        public AssetBundlesService AssetBundles { get; private set; }
        public PrefabManager PrefabManager { get; private set; }
        public BuildingVisualPoolManager BuildingVisualPoolManager { get; private set; }
        public LocalizationService LocalizationService { get; private set; }
        public BuildLimitsManager BuildLimitsManager { get; private set; }

        private bool _isLoaded = false;

        public static ModifScriptsManager Init(Logging.LogLevel logLevel = Logging.LogLevel.Info)
        {
            if (_isAttached)
            {
                Debug.LogWarning($"[{ModInfo.ModName}][{Logging.LogLevel.Warning}] - {nameof(ModifScriptsManager)}.{nameof(Init)} has been called more than once.");
                return _instance;
            }
            
            var scriptGo = new GameObject("ModifScriptsManager");
            scriptGo.SetActive(false);
            
            _instance = scriptGo.AddComponent<ModifScriptsManager>();
            _instance.Logger = new Logging.Logger(logLevel, ModInfo.ModName);
            _instance.ModifGoService = new ModifGoService(() => _instance.Logger);
            var modGo = _instance.ModifGoService.GetModResources(ModInfo.ModId, true);
            scriptGo.transform.SetParent(modGo.transform);
            _isAttached = true;
            //_instance.AttachMonoBehaviours();
            scriptGo.SetActive(true);
            return _instance;
        }

        public void Load()
        {
            AssetBundles.LoadAssetBundles();
            _isLoaded = true;
        }

        private void Awake()
        {
            AssetBundles = new AssetBundlesService(() => Logger);
            //AssetBundles.LoadAssetBundles();
            PrefabManager = new PrefabManager(AssetBundles, () => Logger);
            BuildingVisualPoolManager = new BuildingVisualPoolManager(PrefabManager, () => Logger);
            BuildLimitsManager = new BuildLimitsManager(PrefabManager, () => Logger);
            LocalizationService = new LocalizationService(PrefabManager, () => Logger);
        }

        public void AttachMonoBehaviours()
        {
            var monoBehaviours = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)) && !type.GetTypeInfo().IsAbstract);
            var modGo = ModifGoService.GetModResources(ModInfo.ModId, false);
            var scriptGo = new GameObject("scripts");
            scriptGo.transform.SetParent(modGo.transform);

            foreach (var t in monoBehaviours)
            {
                if (t == typeof(ModifScriptsManager))
                    continue;
#if DEBUG
                Logger.LogTrace($"Attaching script {t.FullName}");
#endif
                scriptGo.AddComponent(t);
            }
        }

        public void AddBuildingBundle(string name, string bundlePath, string localesPath)
        {
            if (_isLoaded)
                throw new InvalidOperationException("Tried to add new Building Bundle after script has loaded.");

            var bundle = new BuildingBundle() { Name = name, BundlePath = bundlePath, LocalesPath = localesPath };
            AssetBundles.AddBuildingBundle(bundle);
            LocalizationService.AddLocalePath(localesPath);
        }
    }
}
