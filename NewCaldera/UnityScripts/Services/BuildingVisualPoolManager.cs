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
        private Logging.Logger Logger => _loggerFactory.Invoke();
        private readonly PrefabManager _prefabManager;

        private bool _buildingVisualPoolAwake;
        private bool _prefabsProcessed;
        private MonoBehaviour _buildingVisualPool;

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
                AttachAllBuildingVisuals(_buildingVisualPool);
                //throw new Exception("Custom Prefabs Loaded After BuildingVisualPool.Awake!");
        }

        public Type GetBuildingVisualPoolType()
        {
            if (_type != null)
                return _type;

            _type = OutwardAssembly.Types.BuildingVisualPool;

            return _type;
        }

        public MonoBehaviour GetBuildingVisualPoolInstance()
        {
            if (_buildingVisualPool != null)
                return _buildingVisualPool;
            _buildingVisualPool = ReflectionExtensions.GetStaticFieldValue<MonoBehaviour>(GetBuildingVisualPoolType(), "m_instance");

            return _buildingVisualPool;
        }


        private void AttachAllBuildingVisuals(MonoBehaviour buildingVisualPool)
        {
            _buildingVisualPoolAwake = true;
            _buildingVisualPool = buildingVisualPool;
            if (!_prefabsProcessed)
                return;
                //throw new Exception("BuildingVisualPool.Awake happened before custom prefab processing complete!");
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

        public MonoBehaviour GetBuildingVisual(int buildingID)
        {
            var types = new Type[] { typeof(int) };
            var method = GetBuildingVisualPoolType().GetMethod("GetVisual", BindingFlags.Public | BindingFlags.Instance, null, types, null);
            var itemVisual = (MonoBehaviour)method.Invoke(GetBuildingVisualPoolInstance(), new object[] { buildingID });
            return itemVisual;
        }

        public void PutbackNewVisual(int buildingID)
        {
            var prefab = _prefabManager.GetCustomBuildingVisualPrefab(buildingID);
            if (prefab == null)
            {
                Logger.LogWarning($"Unable to put back new visual to BuildingVisualPool. Could not find custom building visual for ItemID {buildingID}.");
                return;
            }

            //Destroy any existing visuals.
            var existing = GetBuildingVisual(buildingID);
            while (existing != null)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
                existing = GetBuildingVisual(buildingID);
            }

            //create new visual and store it
            var buildingVisual = UnityEngine.Object.Instantiate(prefab, GetBuildingVisualPoolInstance().transform);
            buildingVisual.name = prefab.name;
            PutbackVisual(buildingID, (MonoBehaviour)buildingVisual.GetComponent(OutwardAssembly.Types.ItemVisual));
        }

        public void PutbackVisual(int buildingID, MonoBehaviour itemVisual)
        {
            var args = new object[] { buildingID, itemVisual };
            var types = new Type[] { typeof(int), OutwardAssembly.Types.ItemVisual };
            var method = GetBuildingVisualPoolType().GetMethod("PutbackVisual", BindingFlags.Public | BindingFlags.Static, null, types, null);
            method.Invoke(null, args);
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
