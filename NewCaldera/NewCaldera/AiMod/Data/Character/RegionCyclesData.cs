using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies;
using ModifAmorphic.Outward.NewCaldera.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Data.Character
{
    internal class RegionCyclesData : JsonDataService<RegionCycles>
    {
        protected override string FileName => "RegionCycles.json";

        private readonly CyclesHydrator _hydrator;

        private bool _isHydrated = false;

        public RegionCyclesData(IDirectoryHandler directoryHandler, CyclesHydrator hydrator, Func<IModifLogger> getLogger) : base(directoryHandler, getLogger)
        {
            _hydrator = hydrator;
        }

        public bool TryGetCycle(string region, out RegionCycle cycle) => GetData().UniqueCycles.TryGetValue(region, out cycle);
        public RegionCycle GetRegionCycle(string region) => GetData().UniqueCycles.TryGetValue(region, out var cycle) ? cycle : null;
        public RegionCycle GetRegionCycle(AreaManager.AreaEnum area) => GetRegionCycle(GetParentRegionName(area));
        public bool TryGetRegionCycle(AreaManager.AreaEnum area, out RegionCycle cycle) => TryGetCycle(GetParentRegionName(area), out cycle);

        public static string GetParentRegionName(AreaManager.AreaEnum area)
        {
            int areaNumber = (int)area;
            if (areaNumber >= 100 && areaNumber <= 151)
                return "Chersonese";
            else if (areaNumber >= 200 && areaNumber <= 251)
                return "Hallowed Marsh";
            else if (areaNumber >= 300 && areaNumber <= 351)
                return "Abrassar";
            else if (areaNumber >= 400 && areaNumber <= 451)
                return "Antique Plateau";
            else if (areaNumber >= 400 && areaNumber <= 451)
                return "Antique Plateau";
            else if ((areaNumber >= 500 && areaNumber <= 550) || areaNumber == (int)AreaManager.AreaEnum.EmercarDungeonsBosses)
                return "Antique Plateau";
            else if (areaNumber >= 600 && areaNumber <= 651)
                return "Caldera";
            else
                return "Unknown Region";
        }

        public bool TryGetUnique(string uid, string region, out UniqueEnemy unique)
        {
            if (TryGetCycle(region, out var cycle))
            {
                if (cycle.UniqueEnemies.TryGetValue(uid, out unique))
                    return true;
            }
            unique = null;
            return false;
        }
        public UniqueEnemy GetUnique(string uid, string region)
        {
            _ = TryGetUnique(uid, region, out var unique);
            return unique;
        }

        public override RegionCycles GetData()
        {
            var cycles = base.GetData();
            bool saveNeeded = false;
            
            if (!_isHydrated)
            {
                if (_hydrator.TryHydrate(cycles))
                    saveNeeded = true;
                _isHydrated = true;
            }

            if (saveNeeded)
            {
                Save();
                cycles = base.GetData();
            }

            if (!_isHydrated)
            {
                _ = _hydrator.TryHydrate(cycles);
                _isHydrated = true;
            }

            return cycles;

        }

        protected override RegionCycles LoadData()
        {
            _isHydrated = false;
            return base.LoadData();
        }

        protected override void ResetCache(bool suppressChangeEvent = false)
        {
            base.ResetCache(suppressChangeEvent);
            _isHydrated = false;
        }

    }
}
