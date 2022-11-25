using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.StaticTables;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models;
using ModifAmorphic.Outward.NewCaldera.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Data
{
    internal class ProbablityTableData : JsonDataService<ItemChanceTables>
    {
        protected override string FileName => "ItemPropabilityTables.json";

        private bool _chanceTablesAdded = false;
        private bool _itemTablesAdded = false;
        private bool _isHydrated = false;

        public ProbablityTableData(IDirectoryHandler directoryHandler, Func<IModifLogger> getLogger) : base(directoryHandler, getLogger)
        {

        }

        public bool TryGetChanceTable(string tableName, out ConditionalChanceTable table) => GetData().ChanceTables.TryGetValue(tableName, out table);

        public ConditionalChanceTable GetChanceTable(string tableName) => GetData().ChanceTables.TryGetValue(tableName, out var table) ? table : null;

        public bool TryGetItemQuantityChance(string tableName, out ItemQuantityChance qtyChanceTable) => GetData().ItemQuantityChances.TryGetValue(tableName, out qtyChanceTable);

        public ItemQuantityChance GetItemQuantityChance(string tableName) => GetData().ItemQuantityChances.TryGetValue(tableName, out var qtyChanceTable) ? qtyChanceTable : null;

        public override ItemChanceTables GetData()
        {
            if (CachedData != null)
            {
                //Logger.LogDebug("Returning cached ItemChanceTables");
                return CachedData;
            }

            CachedData = LoadData();

            if (CachedData == null)
            {
                CachedData = ChanceTables.ItemTables;
                Save();
                CachedData = LoadData();
            }

            bool reloadNeeded = false;
            if (CachedData.ChanceTables == null || CachedData.ChanceTables.Count == 0)
            {
                CachedData.ChanceTables = ChanceTables.ItemTables.ChanceTables;
                reloadNeeded = true;
                _chanceTablesAdded = true;
                Logger.LogDebug("No ChanceTables found. Using default tables.");
            }
            if (CachedData.ItemQuantityChances == null || CachedData.ItemQuantityChances.Count == 0)
            {
                CachedData.ItemQuantityChances = ChanceTables.ItemTables.ItemQuantityChances;
                reloadNeeded = true;
                _itemTablesAdded = true;
                Logger.LogDebug("No ItemQuantityChances found. Using default tables.");
            }

            if (TryAddMissingChanceTables(CachedData.ChanceTables))
                reloadNeeded = true;
            if (TryAddMissingItemTables(CachedData.ItemQuantityChances))
                reloadNeeded = true;

            if (reloadNeeded)
            {
                Save();
                CachedData = LoadData();
            }

            Hydrate(CachedData);

            return CachedData;
        }

        protected override ItemChanceTables LoadData()
        {
            _isHydrated = false;
            return base.LoadData();
        }

        protected override void ResetCache(bool suppressChangeEvent = false)
        {
            base.ResetCache(suppressChangeEvent);
            _isHydrated = false;
        }


        private bool TryAddMissingChanceTables(Dictionary<string, ConditionalChanceTable> chanceTables)
        {
            if (_chanceTablesAdded)
                return false;

            bool tablesAdded = false;
            var missingTables = ChanceTables.ItemTables.ChanceTables.Where(kvp => !chanceTables.ContainsKey(kvp.Key));
            foreach (var kvp in missingTables)
            {
                chanceTables.Add(kvp.Key, kvp.Value);
                tablesAdded = true;
            }
            _chanceTablesAdded = true;
            return tablesAdded;
        }

        private bool TryAddMissingItemTables(Dictionary<string, ItemQuantityChance> itemChances)
        {
            if (_itemTablesAdded)
                return false;

            bool tablesAdded = false;
            var missingTables = ChanceTables.ItemTables.ItemQuantityChances.Where(kvp => !itemChances.ContainsKey(kvp.Key));
            foreach (var kvp in missingTables)
            {
                itemChances.Add(kvp.Key, kvp.Value);
                tablesAdded = true;
            }
            _itemTablesAdded = true;
            return tablesAdded;
        }

        private void Hydrate(ItemChanceTables tables)
        {
            if (_isHydrated)
                return;

            if (tables.ChanceTables != null)
            {
                var tableKeys = tables.ChanceTables.Keys.ToList();
                foreach (var tableName in tableKeys)
                {
                    if (tables.ChanceTables[tableName].WeightedItemTables == null || tables.ChanceTables[tableName].WeightedItemTables.Count == 0)
                        continue;

                    if (tables.ChanceTables[tableName].ItemTables == null)
                        tables.ChanceTables[tableName].ItemTables = new Dictionary<string, ItemQuantityChance>();

                    foreach (var itemTableName in tables.ChanceTables[tableName].WeightedItemTables.Keys)
                    {
                        tables.ChanceTables[tableName].ItemTables.Add(itemTableName, GetItemQuantityChance(itemTableName));
                    }

                }
            }

            _isHydrated = true;
        }
    }
}
