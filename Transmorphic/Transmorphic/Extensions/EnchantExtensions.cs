using System;
using System.Collections.Generic;
using System.Text;
using static EnchantmentRecipe.IngredientData;

namespace ModifAmorphic.Outward.Transmorphic.Extensions
{
    public static class EnchantExtensions
    {
        public static RecipeIngredient.ActionTypes ToActionType(this IngredientType ingredientType)
        {
            if (ingredientType == IngredientType.Specific)
                return RecipeIngredient.ActionTypes.AddSpecificIngredient;

            return RecipeIngredient.ActionTypes.AddGenericIngredient;
        }
    }
}
