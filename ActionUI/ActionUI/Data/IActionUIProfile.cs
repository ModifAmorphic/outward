namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IActionUIProfile
    {
        string Name { get; set; }
        bool ActionSlotsEnabled { get; set; }
        bool DurabilityDisplayEnabled { get; set; }
        bool StashCraftingEnabled { get; set; }
    }
}
