using UnityEditor;
using System.IO;
using System.Linq;
using ModifAmorphic.Outward.UnityScripts;
using UnityEngine;
using System.Collections.Generic;
using ModifAmorphic.Outward.UnityScripts.Models;
using Newtonsoft.Json;
using System.IO.Compression;

public class CreateAssetBundles
{
    private readonly static string AssetBundleDirectory = Path.Combine("Assets", "AssetBundles");

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {

        if (!Directory.Exists(AssetBundleDirectory))
        {
            Directory.CreateDirectory(AssetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(AssetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
    }

    [MenuItem("Assets/Publish Packs/Caldera Foilage")]
    static void PublishCalderaFoilage()
    {
        ConvertCsvLocales.ExportItemDescriptions();
        var pack = BuildingPacksData.GetBuildingPack("Caldera-Foilage");
        PublishPack(pack);
    }

    [MenuItem("Assets/Publish Packs/Hallowed Marsh Foilage")]
    static void PublishHallowedMarshFoilage()
    {
        ConvertCsvLocales.ExportItemDescriptions();
        var pack = BuildingPacksData.GetBuildingPack("HallowedMarsh-Foilage");
        PublishPack(pack);
    }

    [MenuItem("Assets/Publish Packs/Outward Lights")]
    static void PublishNewSiroccoLights()
    {
        ConvertCsvLocales.ExportItemDescriptions();
        var pack = BuildingPacksData.GetBuildingPack("Outward-Lights");
        PublishPack(pack);
    }

    [MenuItem("Assets/Publish Packs/All Packs")]
    static void PublishAll()
    {
        ConvertCsvLocales.ExportItemDescriptions();
        var buildingPacks = BuildingPacksData.GetBuildingPacks();
        foreach (var pack in buildingPacks)
        {
            PublishPack(pack);
        }
    }

    [MenuItem("GameObject/Publish Building Pack", true)]
    static bool ValidateLogSelectedTransformName()
    {
        return Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent<ThunderstoreBuildingPack>(out _);
    }

    [MenuItem("GameObject/Publish Building Pack")]
    static void PublishSelected()
    {
        if (Selection.activeGameObject.TryGetComponent<ThunderstoreBuildingPack>(out var pack))
        {
            ConvertCsvLocales.ExportItemDescriptions();
            PublishPack(pack);
        }
    }

    private static void PublishPack(ThunderstoreBuildingPack pack)
    {
        GenerateBuildingPrefabs.GeneratePrefabs(pack);
        if (pack.BuildAssetBundle)
            BuildAssetBundle(pack);
        PublishMerchantInventories(pack);
        PublishManifests(pack);
        if (pack.CreateZip)
            ZipPack(pack);
    }

    static void BuildAssetBundle(ThunderstoreBuildingPack pack)
    {
        GenerateBuildingPrefabs.GenerateAllPrefabs();

        var itemGuids = AssetDatabase.FindAssets("t:prefab", new string[] { pack.GeneratedItemsPath });
        var itemPaths = itemGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
        Debug.Log($"Adding {itemPaths.Length} Item prefabs to asset bundle '{pack.name}'.");
        var visualGuids = AssetDatabase.FindAssets("t:prefab", new string[] { pack.GeneratedVisualsPath });
        var visualPaths = visualGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
        Debug.Log($"Adding {visualPaths.Length} Item Visual prefabs to asset bundle '{pack.name}'.");

        AssetBundleBuild[] builds = new AssetBundleBuild[2]
        {
            new AssetBundleBuild()
            {
                assetBundleName = pack.FullName,
                assetNames = itemPaths
            },
            new AssetBundleBuild()
            {
                assetBundleName = pack.FullName,
                assetNames = visualPaths
            },
        };

        string bundleDir = Path.Combine(pack.GetOrCreatePackPluginsPath(), "asset-bundles");
        if (!Directory.Exists(bundleDir))
            Directory.CreateDirectory(bundleDir);

        Debug.Log($"Building Asset Bundle to '{Path.Combine(bundleDir, pack.FullName)}'.");
        BuildPipeline.BuildAssetBundles(bundleDir, builds, BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);

        if (File.Exists(Path.Combine(bundleDir, "asset-bundles.manifest")))
            File.Delete(Path.Combine(bundleDir, "asset-bundles.manifest"));
        if (File.Exists(Path.Combine(bundleDir, "asset-bundles")))
            File.Delete(Path.Combine(bundleDir, "asset-bundles"));
        if (File.Exists(Path.Combine(bundleDir, $"{pack.FullName}.manifest")))
            File.Delete(Path.Combine(bundleDir, $"{pack.FullName}.manifest"));
    }

    private static void PublishManifests(ThunderstoreBuildingPack pack)
    {
        var buildingManifest = new BuildingPacksManifest()
        {
            PrefabsPath = pack.GemeratedPrefabsPath.ToLower(),
            LocalesDirectory = pack.LocalesDirectory,
            AssetBundleFilePath = pack.AssetFilePath,
            MerchantInventoryFilePath = pack.MerchantInvManifestFilePath
        };
        var packJson = JsonConvert.SerializeObject(buildingManifest, Formatting.Indented);
        File.WriteAllText(Path.Combine(pack.GetOrCreatePackPluginsPath(), "buildingPacksManifest.json"), packJson);

        
        var manfestJson = JsonConvert.SerializeObject(pack, Formatting.Indented);
        File.WriteAllText(Path.Combine(pack.GetOrCreatePackPath(), "manifest.json"), manfestJson);
    }

    private static void PublishMerchantInventories(ThunderstoreBuildingPack pack)
    {
        var merchants = pack.GetComponentsInChildren<MerchantInventory>();
        var inventories = new MerchantInventoriesHolder();
        foreach (var merchant in merchants)
        {
            if (merchant.GuaranteedItems != null && merchant.GuaranteedItems.Any())
            {
                var merchInv = merchant.ToAdditonalMerchantInventory();
                merchInv.Name = pack.ModName;
                inventories.AdditonalMerchantInventories.Add(merchInv);
            }
        }
        var merchantJson = JsonConvert.SerializeObject(inventories, Formatting.Indented);
        File.WriteAllText(pack.MerchantInvOutputFilePath, merchantJson);
    }

    public static void ZipPack(ThunderstoreBuildingPack pack)
    {
        if (pack.CreateZip)
        {
            if (File.Exists(pack.ZipfilePath))
                File.Delete(pack.ZipfilePath);

            ZipFile.CreateFromDirectory(pack.GetOrCreatePackPath(), pack.ZipfilePath);
        }
    }
}