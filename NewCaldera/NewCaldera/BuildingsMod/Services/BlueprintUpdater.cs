using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Data;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches;
using ModifAmorphic.Outward.NewCaldera.Data;
using ModifAmorphic.Outward.NewCaldera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Services
{
    internal class BlueprintUpdater
    {
        private readonly Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly BlueprintsData _blueprints;
        private readonly BuildingsData _buildingsData;
        private readonly ItemLocalizationsData _localizations;

        public BlueprintUpdater(BlueprintsData blueprints, BuildingsData buildingsData, ItemLocalizationsData localizations, Func<IModifLogger> getLogger)
        {
            (_blueprints, _buildingsData, _localizations, _getLogger) = (blueprints, buildingsData, localizations, getLogger);
            ResourcesPrefabManagerPatches.AfterPrefabsLoaded += ApplyChanges;
            ResourcesPrefabManagerPatches.AfterPrefabsLoaded += ApplyLocalizationChanges;
        }

        private void ApplyChanges(ResourcesPrefabManager prefabManager, ref Dictionary<string, Item> itemPrefabs)
        {
#if DEBUG
            _localizations.DumpLocalizations();
#endif
            foreach (var bp in _blueprints.GetData().Blueprints)
            {
                if (!itemPrefabs.TryGetValue(bp.BuildingItemID.ToString(), out var itemBuilding) || !(itemBuilding is Building building))
                {
                    Logger.LogWarning($"Could not find Building Prefab for {nameof(BuildingBlueprint.BuildingItemID)} {bp.BuildingItemID}, '{bp.Name}'.");
                    continue;
                }
                int firstTierSteps = 0;
                foreach (var step in bp.Steps)
                {
                    if (step.Tier < 2)
                    {
                        firstTierSteps++;
                        ApplyConstructionChanges(building, step);
                    }
                    else
                    {
                        ApplyUpgradeChanges(building, step, step.Step - firstTierSteps - 1);
                    }
                }
            }

            
        }

        private void ApplyLocalizationChanges(ResourcesPrefabManager prefabManager, ref Dictionary<string, Item> itemPrefabs)
        {
            var itemLocs = _localizations.GetData().Items;
            foreach (var loc in itemLocs)
            {
                (var itemID, var localization) = (loc.Key, loc.Value);

                if (!itemPrefabs.TryGetValue(itemID.ToString(), out var bpItem) || !(bpItem is Blueprint blueprint))
                {
                    Logger.LogWarning($"Could not find Blueprint Prefab for ItemID {itemID}. Blueprint description will not be updated.");
                }
                else
                {
                    ApplyBlueprintChanges(blueprint, localization);
                }
            }
            
        }

        private void ApplyConstructionChanges(Building building, BuildingStep step)
        {
            try
            {
                Logger.LogDebug($"Applying construction cost changes to building {building.ItemID} {building.Name}.");
                var phase = building.GetConstructionPhase(step.Step);

                if (step.BuildAmounts != null)
                {
                    var costs = phase.GetConstructionCosts();

                    costs.Food = step.BuildAmounts.FoodAmount;
                    costs.Funds = step.BuildAmounts.FundsAmount;
                    costs.Stone = step.BuildAmounts.StoneAmount;
                    costs.Timber = step.BuildAmounts.TimberAmount;

                    if (step.BuildAmounts.RareItemAmount != null)
                    {
                        phase.RareMaterialRequirement = new ItemQuantity(ResourcesPrefabManager.Instance.GetItemPrefab(step.BuildAmounts.RareItemAmount.ItemID), step.BuildAmounts.RareItemAmount.Amount);
                    }
                    phase.SetPrivateField<Building.ConstructionPhase, BuildingResourceValues>("m_constructionCosts", costs);
                }
                
                phase.SetPrivateField<Building.ConstructionPhase, int>("m_constructionTime", step.BuildDays);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to apply construction cost changes to building {building?.ItemID} {building?.Name}.", ex);
            }
        }

        private void ApplyUpgradeChanges(Building building, BuildingStep step, int upgradePhase)
        {
            try
            {
                Logger.LogDebug($"Applying upgrade phase {upgradePhase} cost changes for building {building.ItemID} {building.Name}.");
                var phase = building.GetUpgradePhase(upgradePhase);
                if (step.BuildAmounts != null)
                {
                    var costs = phase.GetConstructionCosts();

                    costs.Food = step.BuildAmounts.FoodAmount;
                    costs.Funds = step.BuildAmounts.FundsAmount;
                    costs.Stone = step.BuildAmounts.StoneAmount;
                    costs.Timber = step.BuildAmounts.TimberAmount;

                    phase.SetPrivateField<Building.ConstructionPhase, BuildingResourceValues>("m_constructionCosts", costs);
                }

                phase.SetPrivateField<Building.ConstructionPhase, int>("m_constructionTime", step.BuildDays);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to apply upgrade {upgradePhase} cost changes for building {building?.ItemID} {building?.Name}.", ex);
            }
        }

        private void ApplyBlueprintChanges(Blueprint blueprint, ItemLocalization localization)
        {
            //bool isDescChanged = false;
            
            var modBp = _blueprints.GetData().Blueprints
                .FirstOrDefault(b => b.Steps.Any(s => s.BlueprintItemID == blueprint.ItemID));
            var buildSteps = modBp?.Steps?.Where(s => s.BlueprintItemID == blueprint.ItemID);
            var buildStep = buildSteps?.Last();

            var modBuilding = _buildingsData.GetData().Buildings
                .FirstOrDefault(b => b.Tiers.Any(t => t.BlueprintItemID == blueprint.ItemID));
            var tier = modBuilding?.Tiers?.FirstOrDefault(t => t.BlueprintItemID == blueprint.ItemID);

            int buildingItemID = modBp?.BuildingItemID ?? modBuilding?.BuildingItemID ?? -1;
            int buildingTier = buildStep?.Tier ?? tier?.Tier ?? -1;

            Building building = buildingItemID != -1 ? (Building)ResourcesPrefabManager.Instance.GetItemPrefab(buildingItemID) : null;

            if (building != null)
            {
                var description = localization.Description;

                Building.ConstructionPhase[] phases;
                if (buildingTier == 1)
                {
                    phases = building.GetPrivateField<Building, Building.ConstructionPhase[]>("m_constructionPhases");
                }
                else
                {
                    phases = new Building.ConstructionPhase[1] { building.GetUpgradePhase(buildingTier - 2) };
                }

                var costs = new BuildingResourceValues();
                Item rareMaterial = null;
                int buildDays = 0;
                for (int p = 0; p < phases.Length; p++)
                {
                    costs.Food += phases[p].GetConstructionCosts().Food;
                    costs.Funds += phases[p].GetConstructionCosts().Funds;
                    costs.Stone += phases[p].GetConstructionCosts().Stone;
                    costs.Timber += phases[p].GetConstructionCosts().Timber;
                    if (phases[p].RareMaterial != null)
                        rareMaterial = phases[p].RareMaterial;
                    buildDays += phases[p].GetConstructionTime();
                }

                int housingRequired = phases[0].HouseCountRequirements;
                var lastPhase = phases[phases.Length - 1];
                var upkeep = lastPhase.UpkeepCosts;
                var production = lastPhase.UpkeepProductions;
                var capacity = lastPhase.CapacityBonus;

                description = description
                            //Production
                            .Replace("{food_production}", production.Food.ToString())
                            .Replace("{funds_production}", production.Funds.ToString())
                            .Replace("{stone_production}", production.Stone.ToString())
                            .Replace("{timber_production}", production.Timber.ToString())
                            //Capacity Increase
                            .Replace("{food_capacity}", capacity.Food.ToString())
                            //.Replace("{funds_capacity}", capacity.Funds.ToString())
                            .Replace("{stone_capacity}", capacity.Stone.ToString())
                            .Replace("{timber_capacity}", capacity.Timber.ToString())
                            //Costs to Build
                            .Replace("{food_cost}", costs.Food.ToString())
                            .Replace("{funds_cost}", costs.Funds.ToString())
                            .Replace("{stone_cost}", costs.Stone.ToString())
                            .Replace("{timber_cost}", costs.Timber.ToString())
                            .Replace("{house_amount}", housingRequired.ToString())
                            .Replace("{build_days}", buildDays.ToString())
                            //Upkeep
                            .Replace("{food_upkeep}", upkeep.Food.ToString())
                            .Replace("{funds_upkeep}", upkeep.Funds.ToString())
                            .Replace("{stone_upkeep}", upkeep.Stone.ToString())
                            .Replace("{timber_upkeep}", upkeep.Timber.ToString());

                if (rareMaterial != null)
                    description = description.Replace("{item_cost}", rareMaterial.DisplayName);

                if (description != building.Description)
                    _localizations.RegisterLocalization(blueprint.ItemID, null, description);
            }

            //if (tier?.UpkeepAmounts != null)
            //{
            //    description = description
            //                .Replace("{food_production}", tier.ProductionAmounts.FoodAmount.ToString())
            //                .Replace("{funds_production}", tier.ProductionAmounts.FundsAmount.ToString())
            //                .Replace("{stone_production}", tier.ProductionAmounts.StoneAmount.ToString())
            //                .Replace("{timber_production}", tier.ProductionAmounts.TimberAmount.ToString());
            //    isDescChanged = true;
            //}
            //if (tier?.CapacityIncreases != null)
            //{
            //    description = description
            //                .Replace("{food_capacity}", tier.CapacityIncreases.FoodAmount.ToString())
            //                //.Replace("{funds_capacity}", tier.CapacityIncreases.FundsAmount.ToString())
            //                .Replace("{stone_capacity}", tier.CapacityIncreases.StoneAmount.ToString())
            //                .Replace("{timber_capacity}", tier.CapacityIncreases.TimberAmount.ToString());
            //    isDescChanged = true;
            //}
            //if (buildSteps?.Any(s => s.BuildAmounts != null) ?? false)
            //{
            //    var amounts = buildSteps.Where(s => s.BuildAmounts != null).Select(s => s.BuildAmounts);
            //    description = description
            //                .Replace("{food_cost}", amounts.Sum(a => a.FoodAmount).ToString())
            //                .Replace("{funds_cost}", amounts.Sum(a => a.FundsAmount).ToString())
            //                .Replace("{stone_cost}", amounts.Sum(a => a.StoneAmount).ToString())
            //                .Replace("{timber_cost}", amounts.Sum(a => a.TimberAmount).ToString())
            //                .Replace("{house_amount}", buildSteps.First().BuildAmounts.Housing.ToString());
            //    isDescChanged = true;
            //}
            //if (tier?.UpkeepAmounts != null)
            //{
            //    description = description
            //                .Replace("{food_upkeep}", tier.UpkeepAmounts.FoodAmount.ToString())
            //                .Replace("{funds_upkeep}", tier.UpkeepAmounts.FundsAmount.ToString())
            //                .Replace("{stone_upkeep}", tier.UpkeepAmounts.StoneAmount.ToString())
            //                .Replace("{timber_upkeep}", tier.UpkeepAmounts.TimberAmount.ToString());
            //    isDescChanged = true;
            //}

            //if (buildSteps != null)
            //{
            //    var buildDays = buildSteps?.Sum(s => s.BuildDays);
            //    description = description.Replace("{build_days}", buildDays.ToString());
            //    isDescChanged = true;
            //}
            //if (isDescChanged)
            //    _localizations.RegisterLocalization(blueprint.ItemID, null, description);
        }
    }
}
