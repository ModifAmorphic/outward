using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class PrefabManager
    {
        private object _manager = null;
        private Type _type;
        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => ModifScriptsManager.Instance.Logger;
        private readonly AssetBundlesService _assetBundles;
        private IDictionary _item_prefabs;
        private List<GameObject> _delayedBindings = new List<GameObject>();

        private bool _outwardPrefabsLoaded = false;
        private bool _customPrefabsLoaded = false;
        private HashSet<string> _processingBundles = new HashSet<string>();

        //private List<UnityEngine.Object> _boundItems = new List<UnityEngine.Object>();

        private Dictionary<int, GameObject> _itemPrefabs = new Dictionary<int, GameObject>();
        private Dictionary<int, GameObject> _itemVisuals = new Dictionary<int, GameObject>();
        private Dictionary<int, GameObject> _buildingVisuals = new Dictionary<int, GameObject>();

        private bool _customPrefabsProcessed = false;
        public bool IsCustomPrefabsProcessed => _customPrefabsProcessed;

        public event Action OnCustomPrefabsLoaded;
        //private Queue<ItemBinder> _queuedItemBinders = new Queue<ItemBinder>();

        public PrefabManager(AssetBundlesService assetBundles, Func<Logging.Logger> loggerFactory)
        {
            _assetBundles = assetBundles;
            _loggerFactory = loggerFactory;
            TryGetCustomItemVisualPrefab = TryGetItemVisualPrefab;
            PatchResourcesPrefabManager();

            if (_assetBundles.IsBundlesLoaded)
                TryLoadAndBindPrefabs();
            else
                _assetBundles.OnBundlesLoaded += TryLoadAndBindPrefabs;

            OnLoadItemPrefabs += OnLoadItemPrefabsComplete;
        }

        public object GetResourcesPrefabManager()
        {
            if (_manager != null)
                return _manager;

            _manager = ReflectionExtensions.GetStaticFieldValue(GetResourcesPrefabManagerType(), "m_instance");

            return _manager;
        }

        public Type GetResourcesPrefabManagerType()
        {
            if (_type != null)
                return _type;

            _type = OutwardAssembly.Types.ResourcesPrefabManager;

            return _type;
        }

        public IDictionary GetItemPrefabs()
        {
            if (_item_prefabs != null)
                return _item_prefabs;

            _item_prefabs = (IDictionary)ReflectionExtensions.GetStaticFieldValue(_type, "ITEM_PREFABS");
            return _item_prefabs;
        }

        public Dictionary<string, object> GetItemPrefabsCopy() =>
            ReflectionExtensions.Enumerate(GetItemPrefabs()).ToDictionary(kvp => (string)kvp.Key, kvp => kvp.Value);

        public MonoBehaviour GetItemPrefab(int itemID) => (MonoBehaviour)GetResourcesPrefabManager().GetMethodResult(_type, "GetItemPrefab", itemID);

        public GameObject GetCustomBuildingVisualPrefab(int itemID) => _buildingVisuals.TryGetValue(itemID, out var buildingVisual) ? buildingVisual : null;

        public IEnumerable<GameObject> GetCustomBuildingVisualPrefabs() => _buildingVisuals.Values;

        private bool TryGetItemVisualPrefab(string prefabPath, out Transform prefabTransform)
        {
            if (_assetBundles.TryGetPrefabAsset(prefabPath, out var asset))
            {
                prefabTransform = asset.transform;
                return true;
            }

            prefabTransform = null;
            return false;
        }

        private void TryLoadAndBindPrefabs()
        {
            if (_outwardPrefabsLoaded && _assetBundles.IsBundlesLoaded)
            {
                StartAssetLoadAndBind();
            }
        }

        private void OnLoadItemPrefabsComplete()
        {
            _outwardPrefabsLoaded = true;
            TryLoadAndBindPrefabs();
        }

        private void StartAssetLoadAndBind()
        {
            Logger.LogDebug("RegisterBundlesItems");
            foreach (var bundle in _assetBundles.GetAllAssetBundles())
            {
                try
                {
                    _processingBundles.Add(bundle.name);
                    Logger.LogDebug($"Started processing of bundle {bundle.name}.");
                    bundle.LoadAllAssetsAsync<GameObject>().completed += (asyncOp) => TryBindPrefabs((AssetBundleRequest)asyncOp, bundle.name);
                }
                catch (Exception ex)
                {
                    Logger.LogException($"Failed starting async load of assets from asset bundle {bundle?.name}.", ex);
                }
            }
        }

        private bool TryBindPrefabs(AssetBundleRequest bundleRequest, string bundleName)
        {
            try
            {
                BindPrefabs(bundleRequest, bundleName);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to bind prefabs in asset bundle {bundleName}.", ex);
            }
            return false;
        }
        private void BindPrefabs(AssetBundleRequest bundleRequest, string bundleName)
        {
            Logger.LogDebug("RegisterBundlesItems");
            var allAssets = bundleRequest.allAssets.Cast<GameObject>().ToList();


            foreach (var asset in allAssets)
            {
                try
                {
                    //Swap in real shaders
                    SwapShaders(asset);
                }
                catch (Exception ex)
                {
                    Logger.LogException($"Shader swap failed on asset {asset?.name} in bundle {bundleName}.", ex);
                }

                try
                {
                    //process Binders and items first
                    ProcessBinders(asset);
                }
                catch (Exception ex)
                {
                    Logger.LogException($"Binder processing failed on asset {asset?.name} in bundle {bundleName}.", ex);
                }
            }

            foreach (var itemKvp in _itemPrefabs)
            {
                try
                {
                    SetItemVisuals(itemKvp.Key, itemKvp.Value);
                }
                catch (Exception ex)
                {
                    Logger.LogException($"Failed to set loaded visual for item {itemKvp.Value?.name} in bundle {bundleName}.", ex);
                }
            }



            _processingBundles.Remove(bundleName);
            if (_processingBundles.Count == 0)
            {
                _customPrefabsLoaded = true;
                AddPrefabsToResourcesManager();
                BindDelayedAssets();
            }
        }

        private void SwapShaders(GameObject asset)
        {
            var meshRenders = asset.GetComponentsInChildren<Renderer>(true);
            foreach (var mesh in meshRenders)
            {
                foreach (var mat in mesh.materials)
                {
                    if (mat.shader.name.StartsWith("Outward/"))
                    {
                        var shaderName = mat.shader.name.Replace("Outward/", string.Empty);
                        //Logger.LogDebug($"Replacing Asset {asset.name}'s material {mat.shader.name} shader shader '{mat.shader.name}' with shader '{shaderName}'.");
                        var shader = Shader.Find(shaderName);
                        mat.shader = shader;
                    }
                }
            }
        }

        private void ProcessBinders(GameObject asset)
        {
            //process visuals and items first
            if (asset.TryGetComponent<BuildingVisualBinder>(out var buildingVisual))
            {
                buildingVisual.Bind();
                int itemID = buildingVisual.ItemID;
                UnityEngine.Object.Destroy(buildingVisual);
                _buildingVisuals.Add(itemID, asset);
            }
            else if (asset.TryGetComponent<ItemVisualBinder>(out var itemVisual))
            {
                itemVisual.Bind();
                int itemID = itemVisual.ItemID;
                UnityEngine.Object.Destroy(itemVisual);
                _itemVisuals.Add(itemID, asset);
            }
            else if (asset.TryGetComponent<ItemBinder>(out var itemBinder))
            {
                itemBinder.Bind();
                int itemId = itemBinder.ItemID;
                UnityEngine.Object.Destroy(itemBinder);
                _itemPrefabs.Add(itemId, asset);
            }

            if (asset.TryGetComponent<LateScriptBinder>(out _) || asset.GetComponentsInChildren<LateScriptBinder>().Length > 0)
            {
                //Delay binding of other assets until all items are loaded to the ResourcesPrefabManager.ITEM_PREFABs. 
                //This is done in case these other assets have links to items.
                _delayedBindings.Add(asset);
            }
        }

        private void SetItemVisuals(int itemID, GameObject itemGo)
        {
            var itemType = OutwardAssembly.Types.Item;
            var buildingVisualType = OutwardAssembly.Types.BuildingVisual;
            var itemVisualType = OutwardAssembly.Types.ItemVisual;

            //Try to set every Item's m_loadedVisual to the visual
            GameObject visual;
            if (_itemVisuals.TryGetValue(itemID, out visual) || _buildingVisuals.TryGetValue(itemID, out visual))
            {
                if (itemGo.TryGetComponent(itemType, out var item) && item.GetFieldValue(itemType, "m_loadedVisual") == null)
                {
                    if (visual.TryGetComponent(itemVisualType, out var customItemVisual))
                    {
                        item.SetField(itemType, "m_loadedVisual", customItemVisual);
#if DEBUG
                        Logger.LogTrace($"Set {itemType.Name} {item.name}'s m_loadedVisual field to {itemVisualType.Name} {customItemVisual.name}.");
#endif
                    }
                    else if (TryGetItemVisualPrefab(item.GetFieldValue(itemType, "m_visualPrefabPath").ToString(), out var visualTransform))
                    {
                        if (visualTransform.TryGetComponent(itemVisualType, out var itemVisual))
                        {
                            item.SetField(itemType, "m_loadedVisual", itemVisual);
#if DEBUG
                            Logger.LogTrace($"Set {itemType.Name} {item.name}'s m_loadedVisual field to {itemVisualType.Name} {visualTransform.name}.");
#endif
                        }
                    }
                }
            }
        }

        private void AddPrefabsToResourcesManager()
        {
            //if (!(_outwardPrefabsLoaded && _customPrefabsLoaded))
            //    return;

            Logger.LogDebug($"There are {_itemPrefabs.Count} items queued to be added to ITEM_PREFABS.");

            var processItemPrefabs = GetResourcesPrefabManagerType().GetMethod("ProcessItemPrefabs",
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: new Type[2] { typeof(IList<UnityEngine.Object>), typeof(string) },
                modifiers: null);

            processItemPrefabs.Invoke(GetResourcesPrefabManager(), new object[2] { _itemPrefabs.Values.Cast<UnityEngine.Object>().ToList(), string.Empty });

            _customPrefabsProcessed = true;
            OnCustomPrefabsLoaded?.Invoke();
        }

        private void BindDelayedAssets()
        {
            foreach (var asset in _delayedBindings)
            {
                var binders = asset.GetComponents<LateScriptBinder>();
                for (int i = 0; i < binders.Length; i++)
                {
                    if (!binders[i].IsBound)
                    {
                        var component = binders[i].Bind();
#if DEBUG
                        //Logger.LogTrace($"Bound {component.GetType().Name} component to asset {asset.name}.");
#endif
                    }
                    UnityEngine.Object.Destroy(binders[i]);
                }
                var childBinders = asset.GetComponentsInChildren<LateScriptBinder>();
                for (int i = 0; i < childBinders.Length; i++)
                {
                    if (!childBinders[i].IsBound)
                    {
                        var component = childBinders[i].Bind();
#if DEBUG
                        //Logger.LogTrace($"Bound {component.GetType().Name} component to asset {asset.name}.");
#endif
                    }
                    UnityEngine.Object.Destroy(childBinders[i]);
                }
            }

            _delayedBindings.Clear();
        }

        #region Patches
        private void PatchResourcesPrefabManager()
        {
            Logger.LogInfo("Patching ResourcesPrefabManager");

            //Patch GetItemVisualPrefab
            var getItemVisualPrefab = GetResourcesPrefabManagerType().GetMethod("GetItemVisualPrefab");
            var getItemVisualPrefabPostFix = typeof(PrefabManager).GetMethod(nameof(GetItemVisualPrefabPostFix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(getItemVisualPrefab, postfix: new HarmonyMethod(getItemVisualPrefabPostFix));

            //Patch LoadItemPrefabs
            var loadItemPrefabs = GetResourcesPrefabManagerType().GetMethod("LoadItemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
            var loadItemPrefabsPostFix = typeof(PrefabManager).GetMethod(nameof(LoadItemPrefabsPostFix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(loadItemPrefabs, postfix: new HarmonyMethod(loadItemPrefabsPostFix));
        }

        public delegate bool TryGetItemVisualPrefabDelegate(string prefabPath, out Transform prefab);
        public static TryGetItemVisualPrefabDelegate TryGetCustomItemVisualPrefab;

        private static void GetItemVisualPrefabPostFix(string _visualPath, ref Transform __result)
        {
            try
            {
                if (__result != null)
                    return;
#if DEBUG
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(PrefabManager)}::{nameof(GetItemVisualPrefabPostFix)}(): Invoking {nameof(TryGetItemVisualPrefab)} for path '{_visualPath}'.");
#endif
                if (TryGetCustomItemVisualPrefab(_visualPath, out var prefab))
                {
                    __result = prefab;
                }
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(PrefabManager)}::{nameof(GetItemVisualPrefabPostFix)}(): Exception invoking {nameof(TryGetItemVisualPrefab)}.", ex);
            }
        }

        public static event Action OnLoadItemPrefabs;

        private static void LoadItemPrefabsPostFix()
        {
            try
            {
#if DEBUG
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(PrefabManager)}::{nameof(LoadItemPrefabsPostFix)}(): Invoking {nameof(OnLoadItemPrefabs)}.");
#endif
                OnLoadItemPrefabs?.Invoke();
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(PrefabManager)}::{nameof(LoadItemPrefabsPostFix)}(): Exception invoking {nameof(OnLoadItemPrefabs)}.", ex);
            }
        }

        #endregion
    }
}
