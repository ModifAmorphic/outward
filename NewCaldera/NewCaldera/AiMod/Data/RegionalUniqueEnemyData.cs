using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.StaticTables;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models;
using ModifAmorphic.Outward.NewCaldera.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Data
{
    internal class RegionalUniqueEnemyData : JsonDataService<RegionalUniques>
    {
        protected override string FileName => "RegionUniques.json";

        private readonly UniqueEnemyHydrator _hydrator;

        private bool _uniquesChecked = false;
        private bool _isHydrated = false;


        public RegionalUniqueEnemyData(IDirectoryHandler directoryHandler, UniqueEnemyHydrator hydrator, Func<IModifLogger> getLogger) : base(directoryHandler, getLogger)
        {
            _hydrator = hydrator;
        }

        public bool TryGetUnique(string regionName, out Dictionary<string, UniqueEnemy> regionUniques) => GetData().RegionUniques.TryGetValue(regionName, out regionUniques);

        public Dictionary<string, UniqueEnemy> GetRegionUniques(string regionName) => GetData().RegionUniques.TryGetValue(regionName, out var region) ? region : null;

        public override RegionalUniques GetData()
        {
            var regions = base.GetData();
            bool saveNeeded = false;
            if (regions.RegionUniques == null || regions.RegionUniques.Count < 1)
            {
                regions.RegionUniques = UniqueEnemies.AllRegions;
                _uniquesChecked = true;
                saveNeeded = true;
                Logger.LogDebug($"{nameof(RegionalUniqueEnemyData)}::{nameof(GetData)}: Set missing RegionUniques.");
            }

            if (TryAddMissingData(regions))
                saveNeeded = true;

            if (saveNeeded)
            {
                Save();
                regions = base.GetData();
            }

            if (!_isHydrated)
            {
                _hydrator.Hydrate(regions);
                _isHydrated = true;
            }

            return regions;
            
        }

        protected override RegionalUniques LoadData()
        {
            _isHydrated = false;
            return base.LoadData();
        }

        protected override void ResetCache(bool suppressChangeEvent = false)
        {
            base.ResetCache(suppressChangeEvent);
            _isHydrated = false;
        }

        private bool TryAddMissingData(RegionalUniques regions)
        {
            if (_uniquesChecked)
                return false;

            bool dataAdded = false;
            var missingRegions = UniqueEnemies.AllRegions.Where(kvp => !regions.RegionUniques.ContainsKey(kvp.Key));
            foreach (var kvp in missingRegions)
            {
                regions.RegionUniques.Add(kvp.Key, kvp.Value);
                dataAdded = true;
            }

            foreach (var regionName in UniqueEnemies.AllRegions.Keys)
            {
                var uniques = UniqueEnemies.AllRegions[regionName];
                foreach (var unique in uniques)
                {
                    if (!regions.RegionUniques[regionName].ContainsKey(unique.Key))
                    {
                        regions.RegionUniques[regionName].Add(unique.Key, unique.Value);
                        dataAdded = true;
                    }
                }
            }

            Logger.LogDebug($"{nameof(RegionalUniqueEnemyData)}::{nameof(GetData)}: Added {missingRegions.Count()} missing region uniques. Total regions: {regions.RegionUniques.Count}. Default Regions: {UniqueEnemies.AllRegions.Count()}.");
            _uniquesChecked = true;
            return dataAdded;
        }
    }
}
