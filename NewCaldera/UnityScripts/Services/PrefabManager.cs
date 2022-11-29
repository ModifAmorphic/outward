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
                _processingBundles.Add(bundle.name);
                Logger.LogDebug($"Started processing of bundle {bundle.name}.");
                bundle.LoadAllAssetsAsync<GameObject>().completed += (asyncOp) => BindPrefabs((AssetBundleRequest)asyncOp, bundle.name);
            }
        }

        //private void PopulatePrefabs(AssetBundleRequest bundleRequest, string bundleName)
        //{
        //    Logger.LogDebug("RegisterBundlesItems");
        //    var itemType = OutwardAssembly.Types.Item;
        //    var buildingVisualType = OutwardAssembly.Types.BuildingVisual;
        //    var itemVisualType = OutwardAssembly.Types.ItemVisual;

        //    foreach (var asset in bundleRequest.allAssets)
        //    {
        //        var assetGo = (GameObject)asset;
        //        if (assetGo.TryGetComponent(itemType, out var item))
        //            _itemPrefabs.Add((int)item.GetFieldValue(itemType, "ItemID"), assetGo);
        //        else if (assetGo.TryGetComponent(buildingVisualType, out var buildingVisual))
        //            _buildingVisuals.Add((int)buildingVisual.GetFieldValue(buildingVisualType, "m_itemID"), assetGo);
        //        else if (assetGo.TryGetComponent(itemVisualType, out var itemVisual))
        //            _itemVisuals.Add((int)itemVisual.GetFieldValue(itemVisualType, "m_itemID"), assetGo);
        //    }

        //    foreach (var itemKvp in _itemPrefabs)
        //    {
        //        GameObject visual;
        //        if (_itemVisuals.TryGetValue(itemKvp.Key, out visual) || _buildingVisuals.TryGetValue(itemKvp.Key, out visual))
        //        {
        //            if (itemKvp.Value.TryGetComponent(itemType, out var item) && visual.TryGetComponent(itemVisualType, out var itemVisual))
        //            {
        //                item.SetField(itemType, "m_loadedVisual", itemVisual);
        //                Logger.LogTrace($"Set {itemType.Name} {item.name}'s m_loadedVisual field to {itemVisualType.Name} {itemVisual.name}.");
        //            }
        //        }
        //    }

        //    _processingBundles.Remove(bundleName);
        //    if (_processingBundles.Count == 0)
        //    {
        //        _customPrefabsLoaded = true;
        //        ProcessQueuedItemPrefabs();
        //    }
        //}

        private void BindPrefabs(AssetBundleRequest bundleRequest, string bundleName)
        {
            Logger.LogDebug("RegisterBundlesItems");
            var itemType = OutwardAssembly.Types.Item;
            var buildingVisualType = OutwardAssembly.Types.BuildingVisual;
            var itemVisualType = OutwardAssembly.Types.ItemVisual;
            var allAssets = bundleRequest.allAssets.Cast<GameObject>().ToList();

            
            foreach (var asset in allAssets)
            {
                //Swap in real shaders
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

            //var shader = Shader.Find("Custom/Main Set/Main Standard");
            //Logger.LogDebug($"Tried to find shader 'Custom/Main Set/Main Standard'. Found '{shader.name}'.");

            //Try to set every Item's m_loadedVisual to the visual
            foreach (var itemKvp in _itemPrefabs)
            {
                GameObject visual;
                if (_itemVisuals.TryGetValue(itemKvp.Key, out visual) || _buildingVisuals.TryGetValue(itemKvp.Key, out visual))
                {
                    if (itemKvp.Value.TryGetComponent(itemType, out var item) && item.GetFieldValue(itemType, "m_loadedVisual") == null)
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

            _processingBundles.Remove(bundleName);
            if (_processingBundles.Count == 0)
            {
                _customPrefabsLoaded = true;
                AddPrefabsToResourcesManager();
                BindDelayedAssets();
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
                        Logger.LogTrace($"Bound {component.GetType().Name} component to asset {asset.name}.");
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
                        Logger.LogTrace($"Bound {component.GetType().Name} component to asset {asset.name}.");
#endif
                    }
                    UnityEngine.Object.Destroy(childBinders[i]);
                }
            }

            _delayedBindings.Clear();
        }

        //public void AddItemPrefab(ItemBinder itemBinder)
        //{
        //    if (!_outwardPrefabsLoaded)
        //    {
        //        _queuedItemBinders.Enqueue(itemBinder);
        //    }
        //    else
        //    {
        //        string itemId = itemBinder.ItemID.ToString();
        //        if (!GetItemPrefabs().Contains(itemId))
        //            GetItemPrefabs().Add(itemId, itemBinder.BoundComponent);
        //    }
        //}

        //private void RegisterItemsAfterAssetLoad(AssetBundleRequest bundleRequest, string bundleName)
        //{
        //    var assets = bundleRequest.allAssets.Cast<GameObject>();
        //    Logger.LogDebug($"Looking for {nameof(ItemBinder)} components in asset bundle '{bundleRequest}'.");
        //    foreach (var asset in assets)
        //    {
        //        if (asset.TryGetComponent<ItemBinder>(out var itemBinder))
        //        {
        //            Logger.LogTrace($"Found {nameof(ItemBinder)} '{itemBinder.name}' in asset bundle '{bundleName}'.");
        //            //itemBinder.SetIsPrefab(true);
        //            //AddItemPrefab(itemBinder);
        //            //UnityEngine.Object.DontDestroyOnLoad(itemBinder.gameObject);
        //            if (_assetBundles.TryGetPrefabAsset(itemBinder.VisualPrefabPath, out var visualGo))
        //            {
        //                var itemVisualBinder = visualGo.GetComponent<ItemVisualBinder>();
        //                if (itemVisualBinder != null)
        //                {
        //                    itemBinder.BoundComponent.SetField(itemBinder.BoundType, "m_loadedVisual", itemVisualBinder.BoundComponent);
        //                    Logger.LogTrace($"Set {itemBinder.BoundType.Name} '{itemBinder.name}''s Item Visual to '{itemVisualBinder.name}'.");
        //                    //newVisual.SetActive(true);
        //                }
        //                visualGo.SetActive(true);

        //                //visualGo.SetActive(false);
        //                //var newVisual = UnityEngine.GameObject.Instantiate(visualGo);
        //                //var itemVisualBinder = newVisual.GetComponent<ItemVisualBinder>();
        //                //if (itemVisualBinder != null)
        //                //{
        //                //    itemBinder.BoundComponent.SetField(itemBinder.BoundType, "m_loadedVisual", itemVisualBinder.BoundComponent);
        //                //    Logger.LogTrace($"Set {itemBinder.BoundType.Name} '{itemBinder.name}''s Item Visual to '{itemVisualBinder.name}'.");
        //                //    newVisual.SetActive(true);
        //                //}
        //                //visualGo.SetActive(true);
        //            }

        //            _boundItems.Add(itemBinder.BoundComponent.gameObject);
        //        }

        //        //var itemBinders = asset.GetComponentsInChildren<ItemBinder>(true);
        //        //foreach (var binder in itemBinders)
        //        //{
        //        //    Logger.LogTrace($"Found {nameof(ItemBinder)} '{binder.name}' in asset bundle '{bundleName}'.");
        //        //    AddItemPrefab(binder);
        //        //}
        //    }
        //    ProcessQueuedItemPrefabs();
        //}

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
                if (__result != null || !_visualPath.ToLower().StartsWith("assets/modifamorphicprefabs"))
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
