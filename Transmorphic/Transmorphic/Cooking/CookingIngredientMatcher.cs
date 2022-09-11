using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Cooking
{
    internal class CookingIngredientMatcher : IConsumedItemSelector, ICompatibleIngredientMatcher
    {
        public CustomCraftingMenu ParentCraftingMenu { get; set; }

        private Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory();

        public CookingIngredientMatcher(Func<IModifLogger> loggerFactory) => _loggerFactory = loggerFactory;

        public bool TryGetConsumedItems(CompatibleIngredient compatibleIngredient, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems, out List<Item> preservedItems)
        {
            //Necessary to prevent cooking pot from being consumed when learning new recipes
            if (CookingSettings.CookingKitItemIDs.Contains(compatibleIngredient.ItemID))
            {
                //Don't consume the cooking kit
                consumedItems = new List<KeyValuePair<string, int>>();
                preservedItems = compatibleIngredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems").ToList();
                return true;
            }

            consumedItems = default;
            preservedItems = default;
            //Let the base code figure out the consumed items list
            return false;
        }

        public bool TryGetConsumedStaticItems(CompatibleIngredient compatibleIngredient, Guid staticIngredientID, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems, out List<Item> preservedItems)
        {
            if (CookingSettings.CookingKitCorelationID == staticIngredientID)
            {
                //Don't consume the cooking kit
                consumedItems = new List<KeyValuePair<string, int>>();
                preservedItems = compatibleIngredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems").ToList();
                return true;
            }
            //for the other static, just let the base game consume code figure it out
            consumedItems = default;
            preservedItems = default;
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
