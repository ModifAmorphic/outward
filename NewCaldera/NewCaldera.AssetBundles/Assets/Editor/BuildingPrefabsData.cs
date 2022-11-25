using ModifAmorphic.Outward.UnityScripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingPrefabsData
{
    public static IEnumerable<GameObject> GetBuildingsGameObjects()
        => UnityEngine.SceneManagement.SceneManager.GetSceneByName("BuildingPrefabs").GetRootGameObjects().Where(g => g.activeSelf);
    
    public static Dictionary<int, BuildingResources> GetBlueprintResourceVaules()
    {

        var gos = GetBuildingsGameObjects();
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
