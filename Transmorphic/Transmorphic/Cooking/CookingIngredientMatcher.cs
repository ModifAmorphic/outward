using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Cooking
{
    internal class CookingIngredientMatcher : IConsumedItemSelector, ICompatibleIngredientMatcher
    {
        public CustomCraftingMenu ParentCraftingMenu { get; set; }

        public bool TryGetConsumedItems(CompatibleIngredient compatibleIngredient, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems)
        {
            consumedItems = default;
            //Let the base code figure out the consumed items list
            return false;
        }

        public bool TryGetConsumedStaticItems(CompatibleIngredient compatibleIngredient, Guid staticIngredientID, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems)
        {
            if (CookingSettings.CookingKitCorelationID == staticIngredientID)
            {
                //Don't consume the cooking kit
                consumedItems = new List<KeyValuePair<string, int>>();
                return true;
            }
            //for the other static, just let the base game consume code figure it out
            consumedItems = default;
            return false;
        }

        public bool TryMatchRecipeStep(CompatibleIngredient compatibleIngredient, RecipeIngredient recipeStep, out bool isMatch)
        {
            isMatch = false;
            if (!(recipeStep is CustomRecipeIngredient customIngredient) || customIngredient.CustomRecipeIngredientID != CookingSettings.CookingKitCorelationID)
                return false;

            isMatch = CookingSettings.CookingStaticItemIDs.Contains(compatibleIngredient.ItemID);
            return true;

        }
    }
}
