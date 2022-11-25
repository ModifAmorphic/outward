using UnityEditor;
using System.IO;
using System.Linq;
using ModifAmorphic.Outward.UnityScripts;
using UnityEngine;
using System.Collections.Generic;

public class CreateAssetBundles
{
    private static IEnumerable<GameObject> _buildingGos = BuildingPrefabsData.GetBuildingsGameObjects();

    private readonly static string AssetBundleDirectory = Path.Combine("Assets", "AssetBundles");
    private readonly static string AssetPublishDirectory = Path.Combine("..", "Assets", "asset-bundles");

    //private const string BuildingsName = "modifamorphic-buildings";
    //private const string VegetationName = "modifamorphic-vegetation";

    public const string PublishedPrefabsPath = "Assets/ModifAmorphicPrefabs";
    public readonly static string PrefabPublishDirectory = Path.Combine("Assets", "ModifAmorphicPrefabs");


    //public const string BuildingsItemsPath = "Assets/ModifAmorphicPrefabs/Buildings/Items";
    //public const string BuildingsVisualsPath = "Assets/ModifAmorphicPrefabs/Buildings/Visuals";
    //public const string VegetationItemsPath = "Assets/ModifAmorphicPrefabs/Vegetation/Items";
    //public const string VegetationVisualsPath = "Assets/ModifAmorphicPrefabs/Vegetation/Visuals";

    private static GameObject _temporaryHolder;

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

    //[MenuItem("Assets/Publish AssetBundles")]
    //static void PublishActionMenus()
    //{
    //    BuildAllAssetBundles();
    //    File.Copy(Path.Combine(AssetBundleDirectory, BundleName), Path.Combine(PublishDirectory, BundleName), true);
    //}

    [MenuItem("Assets/Publish AssetBundles/Publish Caldera Foilage")]
    static void PublishCalderaFoilage()
    {
        ConvertCsvLocales.ExportItemDescriptions();
        GeneratePrefabs();
        BuildAssetBundle("Caldera-Foilage");
    }

    [MenuItem("Assets/Publish AssetBundles/Publish Hallowed Marsh Foilage")]
    static void PublishHallowedMarshFoilage()
    {
        ConvertCsvLocales.ExportItemDescriptions();
        GeneratePrefabs();
        BuildAssetBundle("HallowedMarsh-Foilage");
    }

    [MenuItem("Assets/Publish All AssetBundles")]
    static void PublishAll()
    {
        ConvertCsvLocales.ExportItemDescriptions();
        GeneratePrefabs();
        foreach(var go in _buildingGos)
            BuildAssetBundle(go.name);
    }



    static void BuildAssetBundle(string bundleName)
    {
        GeneratePrefabs();

        var itemGuids = AssetDatabase.FindAssets("t:prefab", new string[] { PublishedPrefabsPath + "/" + bundleName + "/Items"});
        var itemPaths = itemGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
        Debug.Log($"Adding {itemPaths.Length} Item prefabs to asset bundle '{bundleName}'.");
        var visualGuids = AssetDatabase.FindAssets("t:prefab", new string[] { PublishedPrefabsPath + "/" + bundleName + "/Visuals" });
        var visualPaths = visualGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
        Debug.Log($"Adding {visualPaths.Length} Item Visual prefabs to asset bundle '{bundleName}'.");

        AssetBundleBuild[] builds = new AssetBundleBuild[2]
        {
            new AssetBundleBuild()
            {
                assetBundleName = bundleName,
                assetNames = itemPaths
            },
            new AssetBundleBuild()
            {
                assetBundleName = bundleName,
                assetNames = visualPaths
            },
        };

        BuildPipeline.BuildAssetBundles(AssetPublishDirectory, builds, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }

    [MenuItem("Assets/Generate Prefabs")]
    public static void GeneratePrefabs()
    {
        _temporaryHolder = new GameObject("prefabCreation");
        _temporaryHolder.SetActive(false);

        foreach (var go in _buildingGos)
        {
            string assetBundle = go.name.ToLower();
            string itemsBasePath = PublishedPrefabsPath + $"/{go.name}/Items";
            string visualsBasePath = PublishedPrefabsPath + $"/{go.name}/Visuals";
            string prefabDirectory = Path.Combine(PrefabPublishDirectory, go.name);
            
            if (!Directory.Exists(Path.Combine(prefabDirectory, "Items")))
                Directory.CreateDirectory(Path.Combine(prefabDirectory, "Items"));
            if (!Directory.Exists(Path.Combine(prefabDirectory, "Visuals")))
                Directory.CreateDirectory(Path.Combine(prefabDirectory, "Visuals"));

            //if (go.name == "Buildings")
            //{
            //    assetBundle = "modifamorphic-buildings";
            //    itemsBasePath = BuildingsItemsPath;
            //    visualsBasePath = BuildingsVisualsPath;
            //}
            //else if (go.name == "Vegetation")
            //{
            //    assetBundle = "modifamorphic-vegetation";
            //    itemsBasePath = VegetationItemsPath;
            //    visualsBasePath = VegetationVisualsPath;
            //}
            //else
            //    continue;

            var itemBinders = go.GetComponentsInChildren<ItemBinder>();
            foreach (var binder in itemBinders)
            {
                CreateNewPrefab(binder.gameObject, itemsBasePath + "/" + binder.name + ".prefab", assetBundle);
            }

            var itemVisuals = go.GetComponentsInChildren<BuildingVisualBinder>();
            foreach (var binder in itemVisuals)
            {
                CreateNewPrefab(binder.gameObject, visualsBasePath + "/" + binder.name + ".prefab", assetBundle);
            }
        }

        UnityEngine.Object.DestroyImmediate(_temporaryHolder);
        _temporaryHolder = null;
    }

    private static void CreateNewPrefab(GameObject origPrefab, string path, string assetBundle)
    {
        var tempGo = Object.Instantiate(origPrefab, _temporaryHolder.transform);

        bool prefabSuccess;
        var newPrefab = PrefabUtility.SaveAsPrefabAsset(tempGo, path, out prefabSuccess);
        if (prefabSuccess)
        {
            Debug.Log("Prefab was saved successfully");
        }
        else
            Debug.Log("Prefab failed to save" + prefabSuccess);
        UnityEngine.Object.DestroyImmediate(tempGo);
    }
}