//using ModifAmorphic.Outward.UnityScripts;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//public class GenerateBuildingPrefabs
//{
//    public const string BuildingsItemsPath = "Assets/ModifAmorphicPrefabs/Buildings/Items";
//    public const string BuildingsVisualsPath = "Assets/ModifAmorphicPrefabs/Buildings/Visuals";
//    public const string VegetationItemsPath = "Assets/ModifAmorphicPrefabs/Vegetation/Items";
//    public const string VegetationVisualsPath = "Assets/ModifAmorphicPrefabs/Vegetation/Visuals";

//    private static GameObject _temporaryHolder;
//    [MenuItem("Assets/Generate Prefabs")]
//    public static void GeneratePrefabs()
//    {

//        var gos = BuildingPrefabsData.GetBuildingsGameObjects();

//        _temporaryHolder = new GameObject("prefabCreation");
//        _temporaryHolder.SetActive(false);

//        foreach (var go in gos)
//        {
//            string assetBundle;
//            string itemsBasePath;
//            string visualsBasePath;
//            if (go.name == "Buildings")
//            {
//                assetBundle = "modifamorphic-buildings";
//                itemsBasePath = BuildingsItemsPath;
//                visualsBasePath = BuildingsVisualsPath;
//            }
//            else if (go.name == "Vegetation")
//            {
//                assetBundle = "modifamorphic-vegetation";
//                itemsBasePath = VegetationItemsPath;
//                visualsBasePath = VegetationVisualsPath;
//            }
//            else
//                continue;

//            var itemBinders = go.GetComponentsInChildren<ItemBinder>();
//            foreach(var binder in itemBinders)
//            {
//                CreateNewPrefab(binder.gameObject, itemsBasePath + "/" + binder.name + ".prefab", assetBundle);
//            }

//            var itemVisuals = go.GetComponentsInChildren<BuildingVisualBinder>();
//            foreach (var binder in itemVisuals)
//            {
//                CreateNewPrefab(binder.gameObject, visualsBasePath + "/" + binder.name + ".prefab", assetBundle);
//            }
//        }
        
//        UnityEngine.Object.DestroyImmediate(_temporaryHolder);
//        _temporaryHolder = null;
//    }

//    private static void CreateNewPrefab(GameObject origPrefab, string path, string assetBundle)
//    {
//        var tempGo = Object.Instantiate(origPrefab, _temporaryHolder.transform);

//        bool prefabSuccess;
//        var newPrefab = PrefabUtility.SaveAsPrefabAsset(tempGo, path, out prefabSuccess);
//        if (prefabSuccess)
//        {
//            Debug.Log("Prefab was saved successfully");
//        }
//        else
//            Debug.Log("Prefab failed to save" + prefabSuccess);
//        UnityEngine.Object.DestroyImmediate(tempGo);
//    }
//}
