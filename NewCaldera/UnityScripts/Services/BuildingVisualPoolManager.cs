using HarmonyLib;
using ModifAmorphic.Outward.UnityScripts.Extensions;
using ModifAmorphic.Outward.UnityScripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class BuildingVisualPoolManager
    {
        private Type _type;
        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => ModifScriptsManager.Instance.Logger;
        private readonly PrefabManager _prefabManager;

        private bool _buildingVisualPoolAwake;
        private bool _prefabsProcessed;

        public BuildingVisualPoolManager(PrefabManager prefabManager, Func<Logging.Logger> loggerFactory)
        {
            _prefabManager = prefabManager;
            _loggerFactory = loggerFactory;
            Patch();
            AfterAwake += AttachAllBuildingVisuals;
            prefabManager.OnCustomPrefabsLoaded += OnCustomPrefabsLoaded;
        }

        private void OnCustomPrefabsLoaded()
        {
            _prefabsProcessed = true;
            if (_buildingVisualPoolAwake)
                throw new Exception("Custom Prefabs Loaded After BuildingVisualPool.Awake!");
        }

        public Type GetBuildingVisualPoolType()
        {
            if (_type != null)
                return _type;

            _type = OutwardAssembly.Types.BuildingVisualPool;

            return _type;
        }


        private void AttachAllBuildingVisuals(MonoBehaviour buildingVisualPool)
        {
            _buildingVisualPoolAwake = true;
            if (!_prefabsProcessed)
                throw new Exception("BuildingVisualPool.Awake happened before custom prefab processing complete!");
            var tempGo = new GameObject("temporaryBuildingVisuals");
            tempGo.SetActive(false);

            var visualPrefabs = _prefabManager.GetCustomBuildingVisualPrefabs();
            foreach(var visualPrefab in visualPrefabs)
            {
                var buildingVisual = UnityEngine.Object.Instantiate(visualPrefab, tempGo.transform);
                buildingVisual.name = visualPrefab.name;
                buildingVisual.transform.SetParent(buildingVisualPool.transform);
            }

            UnityEngine.Object.Destroy(tempGo);
        }
        private void AttachBuildingVisuals(MonoBehaviour buildingVisualPool, IEnumerable<GameObject> assets, GameObject tempParent)
        {
            foreach (var asset in assets)
            {
                if (asset.TryGetComponent<BuildingVisualBinder>(out var buildingVisualPrefab))
                {
                    //var buildingVisual = UnityEngine.Object.Instantiate(buildingVisualPrefab.gameObject, tempParent.transform);
                    ////buildingVisual.DeCloneNames();
                    //_ = buildingVisual.GetComponent<BuildingVisualBinder>().BoundComponent;
                    //buildingVisual.transform.SetParent(buildingVisualPool.transform);
                    _ = buildingVisualPrefab.BoundComponent;
                    buildingVisualPrefab.transform.SetParent(buildingVisualPool.transform);
                    Logger.LogDebug($"Added new BuildingVisual '{buildingVisualPrefab.name}' as child of gameobject '{buildingVisualPool.transform.name}'.");
                }
            }
        }

        #region Patches

        private void Patch()
        {
            Logger.LogInfo("Patching BuildingVisualPool");

            //Patch Awake
            var awake = GetBuildingVisualPoolType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            var awakePostFix = typeof(BuildingVisualPoolManager).GetMethod(nameof(AwakePostfix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(awake, postfix: new HarmonyMethod(awakePostFix));
        }

        public delegate void AwakeDelegate(MonoBehaviour buildingVisualPool);
        public static event AwakeDelegate AfterAwake;

        private static void AwakePostfix(MonoBehaviour __instance)
        {
            try
            {
#if DEBUG
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(BuildingVisualPoolManager)}::{nameof(AwakePostfix)}(): Invoking {nameof(AfterAwake)}.");
#endif
                AfterAwake?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(BuildingVisualPoolManager)}::{nameof(AwakePostfix)}(): Exception invoking {nameof(AfterAwake)}.", ex);
            }
        }

        #endregion
    }
}
