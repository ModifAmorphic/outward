using System.Collections.Generic;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models
{
    internal class TieredBuilding
    {
        public int BuildingItemID { get; set; }
        public string Name { get; set; }
        public List<BuildingTier> Tiers { get; set; }
    }
}
