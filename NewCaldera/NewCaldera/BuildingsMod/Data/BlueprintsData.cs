using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models;
using ModifAmorphic.Outward.NewCaldera.Data;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Data
{
    internal class BlueprintsData : JsonDataService<BuildingBlueprints>
    {
        private bool _isLatest = false;
        public BlueprintsData(IDirectoryHandler directoryHandler, Func<IModifLogger> getLogger) : base(directoryHandler, getLogger)
        {
        }

        protected override string FileName => "BuildingBlueprints.json";

        public BuildingBlueprint GetBlueprint(int itemID) => GetData().Blueprints.FirstOrDefault(b => b.BuildingItemID == itemID);

        public override BuildingBlueprints GetData()
        {
            var buildings = base.GetData();

            if (!_isLatest)
                ReSort(buildings);

            return buildings;
        }

        private void ReSort(BuildingBlueprints buildings)
        {
            for (int i = 0; i < buildings.Blueprints.Count; i++)
            {
                buildings.Blueprints[i].Steps = buildings.Blueprints[i].Steps.OrderBy(s => s.Step).ToList();
            }
        }
    }
}
