using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models;
using ModifAmorphic.Outward.NewCaldera.Data;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Data
{
    internal class BuildingsData : JsonDataService<TieredBuildings>
    {
        private bool _isLatest = false;
        public BuildingsData(IDirectoryHandler directoryHandler, Func<IModifLogger> getLogger) : base(directoryHandler, getLogger)
        {
        }

        protected override string FileName => "BuildingTiers.json";

        public TieredBuilding GetBlueprint(int itemID) => GetData().Buildings.FirstOrDefault(b => b.BuildingItemID == itemID);

        public override TieredBuildings GetData()
        {
            var buildings = base.GetData();

            if (!_isLatest)
                ReSort(buildings);

            return buildings;
        }

        private void ReSort(TieredBuildings tb)
        {
            for (int i = 0; i < tb.Buildings.Count; i++)
            {
                tb.Buildings[i].Tiers = tb.Buildings[i].Tiers.OrderBy(t => t.Tier).ToList();
            }
        }
    }
}
