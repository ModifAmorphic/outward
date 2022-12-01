using ModifAmorphic.Outward.UnityScripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GenerateBuildingPrefabs
{

    [MenuItem("Assets/Generate Prefabs/Caldera Foilage")]
    static void PublishCalderaFoilage()
    {
        var pack = BuildingPacksData.GetBuildingPack("Caldera-Foilage");
        GeneratePrefabs(pack);
    }

    [MenuItem("Assets/Generate Prefabs/Hallowed Marsh Foilage")]
    static void PublishHallowedMarshFoilage()
    {
        var pack = BuildingPacksData.GetBuildingPack("HallowedMarsh-Foilage");
        GeneratePrefabs(pack);
    }

    [MenuItem("Assets/Generate Prefabs/Outward Lights")]
    static void PublishNewSiroccoLights()
    {
        var pack = BuildingPacksData.GetBuildingPack("Outward-Lights");
        GeneratePrefabs(pack);
    }

    [MenuItem("Assets/Generate Prefabs/All")]
    public static void GenerateAllPrefabs()
    {
        var buildingPacks = BuildingPacksData.GetBuildingPacks();
        foreach (var pack in buildingPacks)
        {
            GeneratePrefabs(pack);
        }
    }

    public static void GeneratePrefabs(ThunderstoreBuildingPack pack)
    {
        var temporaryHolder = new GameObject($"{pack.FullName}PrefabGeneration");
        temporaryHolder.SetActive(false);
        string assetBundle = pack.name.ToLower();

        if (!Directory.Exists(Path.Combine(pack.GemeratedPrefabsPath, "Items")))
            Directory.CreateDirectory(Path.Combine(pack.GemeratedPrefabsPath, "Items"));
        if (!Directory.Exists(Path.Combine(pack.GemeratedPrefabsPath, "Visuals")))
            Directory.CreateDirectory(Path.Combine(pack.GemeratedPrefabsPath, "Visuals"));

        var oldVisuals = BuildingPacksData.GetPrefabPaths(pack.GeneratedVisualsPath).ToArray();
        Debug.Log($"Destroying {oldVisuals.Length} existing visual prefabs in '{pack.GeneratedVisualsPath}'.");
        for (int i = 0; i < oldVisuals.Length; i++)
        {
            AssetDatabase.DeleteAsset(oldVisuals[i]);
        }

        var oldItems = BuildingPacksData.GetPrefabPaths(pack.GeneratedItemsPath).ToArray();
        Debug.Log($"Destroying {oldItems.Length} existing item prefabs in '{pack.GeneratedItemsPath}'.");
        for (int i = 0; i < oldItems.Length; i++)
        {
            AssetDatabase.DeleteAsset(oldItems[i]);
        }

        var itemVisuals = pack.GetComponentsInChildren<BuildingVisualBinder>();
        Dictionary<int, GameObject> visualPrefabs = new Dictionary<int, GameObject>();
        foreach (var binder in itemVisuals)
        {
            var visualPrefab = CreateNewPrefab(binder.gameObject, pack.GeneratedVisualsPath + "/" + binder.name + ".prefab", assetBundle, temporaryHolder.transform);
            visualPrefabs.Add(binder.ItemID, visualPrefab);
        }

        var itemBinders = pack.GetComponentsInChildren<ItemBinder>();
        foreach (var binder in itemBinders)
        {
            if (visualPrefabs.TryGetValue(binder.ItemID, out var visualPrefab))
            {
                string visualPath = AssetDatabase.GetAssetPath(visualPrefab);
                binder.VisualPrefabPath = visualPath.Replace(".prefab", string.Empty);
            }
            CreateNewPrefab(binder.gameObject, pack.GeneratedItemsPath + "/" + binder.name + ".prefab", assetBundle, temporaryHolder.transform);
        }

        UnityEngine.Object.DestroyImmediate(temporaryHolder);
        temporaryHolder = null;
    }

    private static GameObject CreateNewPrefab(GameObject origPrefab, string path, string assetBundle, Transform parent)
    {
        var tempGo = Object.Instantiate(origPrefab, parent);

        bool prefabSuccess;
        var newPrefab = PrefabUtility.SaveAsPrefabAsset(tempGo, path, out prefabSuccess);
        
        if (prefabSuccess)
        {
            Debug.Log("Prefab was saved successfully");
        }
        else
            Debug.Log("Prefab failed to save" + prefabSuccess);
        UnityEngine.Object.DestroyImmediate(tempGo);
        return newPrefab;
    }
}
