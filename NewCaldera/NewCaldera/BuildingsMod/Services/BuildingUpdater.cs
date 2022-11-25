using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Localization;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Data;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches;
using ModifAmorphic.Outward.NewCaldera.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Services
{
    internal class BuildingUpdater
    {
        private readonly Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly BuildingsData _buildings;

        public BuildingUpdater(BuildingsData buildings, Func<IModifLogger> getLogger)
        {
            (_buildings, _getLogger) = (buildings, getLogger);
            ResourcesPrefabManagerPatches.AfterPrefabsLoaded += ApplyChanges;
        }

        private void ApplyChanges(ResourcesPrefabManager prefabManager, ref Dictionary<string, Item> itemPrefabs)
        {
            foreach (var b in _buildings.GetData().Buildings)
            {
                if (!itemPrefabs.TryGetValue(b.BuildingItemID.ToString(), out var itemBuilding) || !(itemBuilding is Building building))
                {
                    Logger.LogWarning($"Could not find Building Prefab for {nameof(BuildingBlueprint.BuildingItemID)} {b.BuildingItemID}, '{b.Name}'.");
                    continue;
                }
               
                int firstTierSteps = 0;
                foreach (var tier in b.Tiers)
                {
                    if (tier.Tier == 1)
                    {
                        firstTierSteps++;
                        
                        ApplyBuildingChanges(building, building.GetConstructionPhase(building.PhaseCount - 1), tier);
                    }
                    else if (tier.Tier > 1)
                    {
                        ApplyBuildingChanges(building, building.GetUpgradePhase(tier.Tier - 2), tier);
                    }
                }
            }
        }


        private void ApplyBuildingChanges(Building building, Building.ConstructionPhase phase, BuildingTier tier)
        {
            try
            {
                Logger.LogDebug($"Applying tier {tier.Tier} upkeep cost and production amount changes to building {building.ItemID} {building.Name}.");
                if (tier.UpkeepAmounts != null)
                {
                    phase.UpkeepCosts.Clear();
                    phase.UpkeepCosts.Funds = tier.UpkeepAmounts.FundsAmount;
                    phase.UpkeepCosts.Food = tier.UpkeepAmounts.FoodAmount;
                    phase.UpkeepCosts.Stone = tier.UpkeepAmounts.StoneAmount;
                    phase.UpkeepCosts.Timber = tier.UpkeepAmounts.TimberAmount;
                }
                if (tier.ProductionAmounts != null)
                {
                    phase.UpkeepProductions.Clear();
                    phase.UpkeepProductions.Funds = tier.ProductionAmounts.FundsAmount;
                    phase.UpkeepProductions.Food = tier.ProductionAmounts.FoodAmount;
                    phase.UpkeepProductions.Stone = tier.ProductionAmounts.StoneAmount;
                    phase.UpkeepProductions.Timber = tier.ProductionAmounts.TimberAmount;
                }

                if (tier.CapacityIncreases != null)
                {
                    phase.CapacityBonus.Clear();
                    phase.CapacityBonus.Funds = tier.CapacityIncreases.FundsAmount;
                    phase.CapacityBonus.Food = tier.CapacityIncreases.FoodAmount;
                    phase.CapacityBonus.Stone = tier.CapacityIncreases.StoneAmount;
                    phase.CapacityBonus.Timber = tier.CapacityIncreases.TimberAmount;
                }
                }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to apply tier {tier?.Tier} upkeep cost and production amount changes to building {building?.ItemID} {building?.Name}.", ex);
            }
        }
    }
}
