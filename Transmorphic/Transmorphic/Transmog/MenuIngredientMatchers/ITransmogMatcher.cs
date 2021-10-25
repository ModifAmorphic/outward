using ModifAmorphic.Outward.Modules.Crafting;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Transmorphic.Transmog.MenuIngredientMatchers
{
    interface ITransmogMatcher
    {
        bool IsMatch<T>(T recipe, Tag recipeTag, int ingredientItemID, IEnumerable<Item> ingredientItems) where T : CustomRecipe;
        bool IsRecipeTag(Tag recipeTag);
    }
}