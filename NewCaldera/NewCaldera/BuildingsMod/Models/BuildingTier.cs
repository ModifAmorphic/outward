namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models
{
    internal class BuildingTier
    {
        public int Tier { get; set; }
        public int BlueprintItemID { get; set; }
        public ResourceAmounts UpkeepAmounts { get; set; }
        public ResourceAmounts ProductionAmounts { get; set; }
        public ResourceAmounts CapacityIncreases { get; set; }
    }
}
