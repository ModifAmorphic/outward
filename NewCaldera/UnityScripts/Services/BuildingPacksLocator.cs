using ModifAmorphic.Outward.UnityScripts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class BuildingPacksLocator
    {
        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => ModifScriptsManager.Instance.Logger;
        private readonly string _rootPath;

        public BuildingPacksLocator(string rootPath, Func<Logging.Logger> loggerFactory)
        {
            _rootPath = rootPath;
            _loggerFactory = loggerFactory;
        }

        public List<BuildingPacksManifest> FindManifests()
        {
            var manifests = new List<BuildingPacksManifest>();
            var files = Directory.GetFiles(_rootPath, "buildingPacksManifest.json", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (TryLoadManifest(files[i], out var manifest))
                {
                    LocalizePaths(Path.GetDirectoryName(files[i]),  manifest);
                    manifests.Add(manifest);
                }
            }

            return manifests;
        }

        public bool TryLoadManifest(string manifestFile, out BuildingPacksManifest manifest)
        {
            try
            {
                string json = File.ReadAllText(manifestFile);
                manifest = JsonConvert.DeserializeObject<BuildingPacksManifest>(json);
                return manifest != null;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to load Building Pack Manifest '{manifestFile}'.", ex);
                manifest = null;
                return false;
            }
        }

        private void LocalizePaths(string manifestDir, BuildingPacksManifest manifest)
        {
            if (TryLocalizePath(manifest.AssetBundleFilePath, out var assetPath))
                manifest.AssetBundleFilePath = assetPath;

            if (TryLocalizePath(manifest.LocalesDirectory, out var localesPath))
                manifest.LocalesDirectory = localesPath;

            manifest.AssetBundleFilePath = Path.Combine(manifestDir, manifest.AssetBundleFilePath);
            manifest.LocalesDirectory = Path.Combine(manifestDir, manifest.LocalesDirectory);
            manifest.MerchantInventoryFilePath = Path.Combine(manifestDir, manifest.MerchantInventoryFilePath);
        }

        private bool TryLocalizePath(string path, out string localizedPath)
        {
            var paths = path.Split('\\');
            if (paths.Length == 0)
                paths = path.Split('/');

            if (paths.Length > 0)
            {
                localizedPath = string.Join(Path.DirectorySeparatorChar.ToString(), paths);
                Logger.LogDebug($"Localized path: {localizedPath}");
                return true;
            }
            localizedPath = string.Empty;
            return false;
        }
    }
}
