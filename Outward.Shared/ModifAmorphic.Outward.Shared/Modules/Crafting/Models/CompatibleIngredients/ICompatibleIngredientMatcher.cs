using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients
{
    public interface ICompatibleIngredientMatcher
    {
        CustomCraftingMenu ParentCraftingMenu { get; set; }
        /// <summary>
        /// Invoked to match potential ingredients to the current selected crafting recipe. The core crafting menu code only tracks ItemIDs for this process.
        /// This means, if the <paramref name="compatibleIngredient"/> has more than one owned Item, a positive match will need to be returned if any item is a 
        /// match. In the case where both a match and non match exist, the correct result can then be selected with a <see cref="IConsumedItemSelector"/> during crafting.
        /// </summary>
        /// <param name="compatibleIngredient">The ingredient to match.</param>
        /// <param name="recipeStep">The Recipe filter criteria to match against.</param>
        /// <returns></returns>
        bool MatchRecipeStep(CompatibleIngredient compatibleIngredient, RecipeIngredient recipeStep);
    }
}
