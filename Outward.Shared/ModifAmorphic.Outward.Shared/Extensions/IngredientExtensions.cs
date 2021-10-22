using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Extensions
{
    public static class IngredientExtensions
    {
        public static RecipeIngredient GetRecipeIngredient(this IngredientSelector selector)
        {
            return selector.GetPrivateField<IngredientSelector, RecipeIngredient>("m_refRecipeIngredient");
        }

        public static List<Item> GetOwnedItems(this CompatibleIngredient ingredient)
        {
            return ingredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");
        }
        public static void SetOwnedItems(this CompatibleIngredient ingredient, List<Item> ownedItems)
        {
            ingredient.SetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems", ownedItems);
        }
    }
}
