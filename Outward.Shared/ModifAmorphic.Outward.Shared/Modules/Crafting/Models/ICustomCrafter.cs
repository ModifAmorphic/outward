namespace ModifAmorphic.Outward.Modules.Crafting
{
    public interface ICustomCrafter
    {
        bool TryCraftItem(Recipe recipe, ItemReferenceQuantity recipeResult, out Item item, out bool tryEquipItem);
    }
}
