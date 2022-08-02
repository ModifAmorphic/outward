namespace ModifAmorphic.Outward.Modules.Crafting
{
    public interface IRecipeVisibiltyController
    {
        bool GetRecipeIsVisible(CustomCraftingMenu menu, Recipe recipe);
    }
}
