using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients
{
    public interface ICompatibleIngredientMatcher
    {
        bool MatchRecipeStep(CompatibleIngredient compatibleIngredient, RecipeIngredient _recipeStep);
    }
}
