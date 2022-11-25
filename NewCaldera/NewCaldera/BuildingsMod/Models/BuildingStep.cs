namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Models
{
    internal class BuildingStep
    {
        public int BuildDays { get; set; }
        public int Step { get; set; }
        public int Tier { get; set; }
        public int BlueprintItemID { get; set; }
        public ResourceAmounts BuildAmounts { get; set; }
    }
}
