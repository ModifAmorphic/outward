using System.Collections.Generic;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models
{
    internal class BuildingBlueprint
    {
        public Building.BuildingTypes BuildingType { get; set; }
        public int BuildingItemID { get; set; }
        public string Name { get; set; }
        public List<BuildingStep> Steps { get; set; }
    }
}
