using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.Data.Character;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Conditions;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies;
using ModifAmorphic.Outward.NewCaldera.AiMod.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Services
{
    internal class DropService : IDisposable
    {
        private readonly Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly RegionCyclesData _enemyCyclesData;
        private readonly Random _random = new Random();
        private bool disposedValue;

        public DropService(RegionCyclesData enemyCyclesData, Func<IModifLogger> getLogger)
        {
            (_enemyCyclesData, _getLogger) = (enemyCyclesData, getLogger);
            LootableOnDeathPatches.AfterOnDeath += AfterDeath;
            DropablePatches.IsGenerateDropsAllowed = AllowBaseDropsGenerate;
        }

        private bool AllowBaseDropsGenerate(Dropable dropable, ItemContainer itemContainer)
        {
            if (itemContainer?.OwnerCharacter == null)
                return true;

            var area = AreaManager.Instance.GetCurrentAreaEnum();
            if (!_enemyCyclesData.TryGetRegionCycle(area, out var cycle))
            {
                Logger.LogDebug($"Area '{area}' not tracked. Allowing base drop generation to continue.");
                return true;
            }

            if (cycle.UniqueUidIndex.ContainsKey(itemContainer.OwnerCharacter.UID))
            {
                Logger.LogDebug($"Unique UID '{itemContainer.OwnerCharacter.UID}' was found for area '{area}'. Disabling base drop generation.");
                return false;
            }

            Logger.LogDebug($"Unique UID '{itemContainer.OwnerCharacter.UID}' not found for area '{area}'. Allowing base drop generation to continue.");
            return true;
        }

        private void AfterDeath(LootableOnDeath lootableOnDeath, bool loadedDead)
        {
            if (loadedDead)
                return;

            var area = AreaManager.Instance.GetCurrentAreaEnum();
            Logger.LogDebug($"Area: {area}");
            if (!_enemyCyclesData.TryGetRegionCycle(area, out var cycle))
                return;


            if (cycle.UniqueUidIndex.TryGetValue(lootableOnDeath.Character?.UID, out var uniqueName) && cycle.UniqueEnemies.TryGetValue(uniqueName, out var unique))
            {
                if (unique.PreviousDeaths.Count > 0)
                {
                    Logger.LogDebug($"Rolling drops for {uniqueName}.");
                    AddDrops(unique, cycle, lootableOnDeath.Character?.Inventory?.Pouch);
                }
            }
        }

        public void AddDrops(UniqueEnemy uniqueEnemy, RegionCycle cycle, ItemContainer targetContainer)
        {
            var items = new List<(int itemID, int Qty)>();
            foreach (var dropTable in uniqueEnemy.DropTables.Values)
            {
                var conditionsPassed = true;
                //Check drop conditions are met
                foreach (var condition in dropTable.DropConditions)
                {
                    if (!IsConditionMet(condition, uniqueEnemy, cycle))
                        conditionsPassed = false;
                }
                //
                if (!conditionsPassed)
                {
                    Logger.LogDebug($"{dropTable.Name} conditions not met.");
                    continue;
                }

                Logger.LogDebug($"{dropTable.Name} conditions met. Rolling for drops.");

                //Automatically roll and add items up to the minimum item tables specified.
                var itemChances = new List<ItemQuantityChance>();
                for (int i = 1; i <= dropTable.MinItemTables; i++)
                {
                    var weightTable = RollWeights(dropTable.WeightedItemTables);
                    itemChances.Add(dropTable.ItemTables[weightTable]);
                }

                //Roll for potential additonal item drops up to the max
                for (int i = dropTable.MinItemTables + 1; i <= dropTable.MaxItemTables; i++)
                {
                    if (Roll(dropTable.QuantityChance))
                    {
                        var weightTable = RollWeights(dropTable.WeightedItemTables);
                        itemChances.Add(dropTable.ItemTables[weightTable]);
                        Logger.LogDebug($"Roll succeeded for adding item table {dropTable.ItemTables[weightTable].Name}");
                    }
                }

                //Roll for quantity of individual item drop
                foreach (var itemRoll in itemChances)
                {
                    int itemQty = itemRoll.MinQuantity;

                    for (int i = itemRoll.MinQuantity + 1; i <= itemRoll.MaxQuantity; i++)
                    {
                        if (Roll(itemRoll.QuantityChance))
                            itemQty++;
                    }
                    Logger.LogDebug($"Roll for Item {itemRoll.ItemID} quantity was {itemQty}.");
                    items.Add((itemRoll.ItemID, itemQty));
                }
            }

            if (targetContainer == null)
                return;

            //Generate items and add them to the container.
            foreach (var itemQty in items)
            {
                for (int i = 0; i < itemQty.Qty;i++ )
                {
                    var item = ItemManager.Instance.GenerateItemNetwork((int)itemQty.itemID);
                    item.ChangeParent(targetContainer.transform);
                }
            }
        }

        public bool IsConditionMet(Condition condition, UniqueEnemy uniqueEnemy, RegionCycle cycle)
        {
            if (condition is NotFirstDeathCondition nfdCondition)
            {
                Logger.LogDebug($"Condition {condition.Name}; Unique '{uniqueEnemy.Name}' has previous deaths: {uniqueEnemy.PreviousDeaths.Any()}.");
                return uniqueEnemy.PreviousDeaths.Any();
            }
            else if (condition is UniquesKilledCondition ukCondition && ukCondition.NumericConditions.TryGetValue(UniquesKilledCondition.UniquesKilledKey, out var killed))
            {
                Logger.LogDebug($"Condition {condition.Name} requirement: {killed}; Uniques killed this cycle: {cycle.GetUniquesKilledAmount()}.");
                return cycle.GetUniquesKilledAmount() >= killed;
            }
            else if (condition is PlayerDeathsCondition pdCondition && pdCondition.NumericConditions.TryGetValue(PlayerDeathsCondition.PlayerDeathsKey, out var deaths))
            {
                Logger.LogDebug($"Condition {condition.Name} requirement: {deaths}; Player deaths this cycle: {cycle.GetUniquesKilledAmount()}.");
                return cycle.GetPlayerDeathsAmount() >= deaths;
            }

            return false;
        }

        public string RollWeights(Dictionary<string, decimal> weightedItems)
        {
            var weightSum = weightedItems.Values.Sum();
            var roll = (decimal)_random.NextDouble() * weightSum;

            foreach (var weightKvp in weightedItems)
            {
                roll -= weightKvp.Value;
                if (roll <= 0)
                    return weightKvp.Key;
            }
            throw new ArgumentOutOfRangeException(nameof(weightedItems), "Invalid weighted items passed. Roll was unsuccesfull.");
        }

        public bool Roll(decimal chancePercent)
        {
            var roll = (decimal)_random.NextDouble();
            Logger.LogDebug($"Rolled {roll} against chance {chancePercent}. Roll was {(roll <= chancePercent ? "" : "un")}successful.");
            return roll <= chancePercent;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LootableOnDeathPatches.AfterOnDeath -= AfterDeath;
                    DropablePatches.IsGenerateDropsAllowed = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DropService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
