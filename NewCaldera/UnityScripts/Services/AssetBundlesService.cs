﻿using ModifAmorphic.Outward.UnityScripts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class AssetBundlesService
    {

        public Dictionary<string, BuildingPacksManifest> _buildingBundles = new Dictionary<string, BuildingPacksManifest>();

        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => _loggerFactory.Invoke();

        public Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

        private bool _loadStarted = false;
        private HashSet<string> _loadingBundles = new HashSet<string>();

        public bool IsBundlesLoaded => _loadStarted && !_loadingBundles.Any();

        public event Action OnBundlesLoaded;

        public AssetBundlesService(Func<Logging.Logger> loggerFactory) => _loggerFactory = loggerFactory;

        public void AddBuildingBundle(BuildingPacksManifest bundle)
        {
            _loadStarted = false;
            _buildingBundles.Add(bundle.PrefabsPath, bundle);
            //LoadAssetBundles();
        }

        //public void LoadAssetBundles()
        //{
        //    _loadStarted = true;
        //    var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    foreach (var assetPaths in AssetPaths)
        //    {
        //        string bundlePath = assetPaths.Value.Select(p => Path.Combine(assemblyPath, p)).FirstOrDefault(p => File.Exists(p));

        //        if (bundlePath == default)
        //        {
        //            Logger.LogDebug($"Asset bundle {assetPaths.Key} not found.");
        //            continue;
        //        }

        //        _loadingBundles.Add(assetPaths.Key);
        //        Logger.LogDebug($"Loading Asset Bundle {assetPaths.Key} from location '{bundlePath}'.");
        //        AssetBundle.LoadFromFileAsync(bundlePath).completed += (asyncOperation) => AddAssetBundle(assetPaths.Key, asyncOperation);
        //    }
        //}


        public void LoadAssetBundles()
        {
            _loadStarted = true;
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var bundle in _buildingBundles)
            {
                _loadingBundles.Add(bundle.Key);
                Logger.LogDebug($"Loading Asset Bundle {bundle.Key} from location '{bundle.Value.AssetBundleFilePath}'.");
                AssetBundle.LoadFromFileAsync(bundle.Value.AssetBundleFilePath).completed += (asyncOperation) => AddAssetBundle(bundle.Key, asyncOperation);
            }
            _buildingBundles.Clear();
        }

        public bool TryGetAssetBundle(string bundleKey, out AssetBundle assetBundle)
        {
            if (LoadedBundles.TryGetValue(bundleKey, out assetBundle))
                return true;

            return false;
        }

        public IEnumerable<AssetBundle> GetAllAssetBundles() => LoadedBundles.Values;

        public bool TryGetPrefabAsset(string assetPath, out GameObject asset)
        {
            asset = null;

            if (!IsBundlesLoaded)
            {
                Logger.LogWarning($"Request to get asset '{assetPath}' was requested before AssetBundles were loaded.");
                return false;
            }

            AssetBundle bundle = null;
            foreach(var prefabsPath in LoadedBundles.Keys)
            {
                if (assetPath.StartsWith(prefabsPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    bundle = LoadedBundles[prefabsPath];
                    break;
                }
            }
            if (bundle == null)
            {
                Logger.LogDebug($"No Assetbundle found for path '{assetPath}'.");
                return false;
            }

            var prefabPath = assetPath + ".prefab";
            if (bundle.Contains(prefabPath))
            {
                asset = bundle.LoadAsset<GameObject>(prefabPath);
                return true;
            }

            return false;
        }

        private void AddAssetBundle(string bundleKey, AsyncOperation asyncOperation)
        {
            if (!(asyncOperation is AssetBundleCreateRequest bundleCreate))
                return;

            LoadedBundles.Add(bundleKey, bundleCreate.assetBundle);
            //LoadedBundles[bundleKey].LoadAllAssetsAsync<GameObject>().completed += (asyncOp) => BindAssets(bundleKey, (AssetBundleRequest)asyncOp);
#if DEBUG
            var allAssetNames = bundleCreate.assetBundle.GetAllAssetNames();
            var logAssets = "";
            foreach (string name in allAssetNames)
            {
                logAssets += "\n\t" + name;
            }
            Logger.LogDebug($"Assets in {bundleCreate.assetBundle.name} Bundle: " + logAssets);
#endif
            _loadingBundles.Remove(bundleKey);
            if (_loadingBundles.Count == 0)
            {
                Logger.LogInfo($"Finished loading {LoadedBundles.Count} asset bundles.");
                OnBundlesLoaded?.Invoke();
            }
        }

        //private void BindAssets(string bundleKey, AssetBundleRequest bundleRequest)
        //{
        //    foreach (var asset in bundleRequest.allAssets)
        //    {
        //        var assetGo = (GameObject)asset;
        //        var binders = assetGo.GetComponents<LateScriptBinder>();
        //        for (int i = 0; i < binders.Length; i++)
        //        {
        //            if (!binders[i].IsBound)
        //            {
        //                var component = binders[i].Bind();
        //                Logger.LogTrace($"Bound {component.GetType().Name} component to asset {assetGo.name}.");
        //            }
        //            UnityEngine.Object.Destroy(binders[i]);
        //        }
        //    }

        //    _loadingBundles.Remove(bundleKey);
        //    if (_loadingBundles.Count == 0)
        //    {
        //        Logger.LogInfo($"Finished loading {LoadedBundles.Count} asset bundles.");
        //        OnBundlesLoaded?.Invoke();
        //    }
        //}
    }
}