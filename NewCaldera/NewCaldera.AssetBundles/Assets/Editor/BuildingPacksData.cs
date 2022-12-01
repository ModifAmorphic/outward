using ModifAmorphic.Outward.UnityScripts;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildingPacksData
{
    public static IEnumerable<ThunderstoreBuildingPack> GetBuildingPacks()
        => UnityEngine.SceneManagement.SceneManager.GetSceneByName("BuildingPrefabs").GetRootGameObjects()
            .Where(g => g.activeSelf && g.TryGetComponent<ThunderstoreBuildingPack>(out _))
            .Select(g => g.GetComponent<ThunderstoreBuildingPack>());

    public static ThunderstoreBuildingPack GetBuildingPack(string gameObjectName) => 
        UnityEngine.SceneManagement.SceneManager
            .GetSceneByName("BuildingPrefabs")
            .GetRootGameObjects()
            .FirstOrDefault(p => p.name.Equals(gameObjectName, System.StringComparison.InvariantCultureIgnoreCase))
            .GetComponent<ThunderstoreBuildingPack>();

    public static IEnumerable<string> GetPrefabPaths(string prefabsPath)
    {
        var prefabGuids = AssetDatabase.FindAssets("t:prefab", new string[] { prefabsPath });
        return prefabGuids.Select(g => AssetDatabase.GUIDToAssetPath(g));
    }

    public static IEnumerable<GameObject> GetPrefabs(string prefabsPath)
    {
        var prefabPaths = GetPrefabPaths(prefabsPath);
        return prefabPaths.Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p));
    }

    public static Dictionary<int, BuildingResources> GetBlueprintResourceVaules()
    {

        var gos = GetBuildingPacks();
        var blueprintCosts = new Dictionary<int, BuildingResources>();
        var tempHolder = new GameObject("TemporaryHolder");
        tempHolder.SetActive(false);
        foreach (var go in gos)
        {
            //var bluePrints = go.GetComponentsInChildren<BlueprintBinder>();
            var buildings = go.GetComponentsInChildren<BuildingBinder>();
            foreach (var building in buildings)
            {
                //var buildingGo = Object.Instantiate(buildingBinder.gameObject, tempHolder.transform);
                //var building = buildingGo.GetComponent<BuildingBinder>();
                var br = new BuildingResources();
                foreach (var phase in building.ConstructionPhases)
                {
                    Debug.Log("Phase: " + phase.DebugName);
                    if (phase.ConstructionCosts != null)
                    {
                        br.BuildingCosts.Funds += phase.ConstructionCosts.Funds;
                        br.BuildingCosts.Food += phase.ConstructionCosts.Food;
                        br.BuildingCosts.Stone += phase.ConstructionCosts.Stone;
                        br.BuildingCosts.Timber += phase.ConstructionCosts.Timber;
                        if (phase.RareMaterialRequirement.ItemID != -1)
                        {
                            br.ItemCosts.Add(
                                new ItemQuantityBinder()
                                {
                                    ItemID = phase.RareMaterialRequirement.ItemID,
                                    Quantity = phase.RareMaterialRequirement.Quantity
                                });
                        }
                    }

                    if (phase.UpkeepCosts != null)
                    {
                        br.UpkeepCosts.Funds += phase.UpkeepCosts.Funds;
                        br.UpkeepCosts.Food += phase.UpkeepCosts.Food;
                        br.UpkeepCosts.Stone += phase.UpkeepCosts.Stone;
                        br.UpkeepCosts.Timber += phase.UpkeepCosts.Timber;
                    }

                    if (phase.UpkeepProductions != null)
                    {
                        br.ProductionAmounts.Funds += phase.UpkeepProductions.Funds;
                        br.ProductionAmounts.Food += phase.UpkeepProductions.Food;
                        br.ProductionAmounts.Stone += phase.UpkeepProductions.Stone;
                        br.ProductionAmounts.Timber += phase.UpkeepProductions.Timber;
                        br.HousingIncrease += phase.HousingValue;
                    }

                    if (phase.CapacityBonus != null)
                    {
                        br.CapacityIncrease.Funds = phase.CapacityBonus.Funds;
                        br.CapacityIncrease.Food = phase.CapacityBonus.Food;
                        br.CapacityIncrease.Stone = phase.CapacityBonus.Stone;
                        br.CapacityIncrease.Timber = phase.CapacityBonus.Timber;
                    }
                }
                Debug.Log($"Adding Blueprint costs for building {building.name}, Blueprint ItemID: {building.ItemID + 1}");
                blueprintCosts.Add(building.ItemID + 1, br);
                //Object.DestroyImmediate(building);
            }
        }
        //UnityEngine.Object.DestroyImmediate(tempHolder);
        return blueprintCosts;        
    }
}
