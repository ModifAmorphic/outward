using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Ingredients
{
    internal class EnchantIngredientMatcher : IConsumedItemSelector, ICompatibleIngredientMatcher
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public CustomCraftingMenu ParentCraftingMenu { get; set; }

        public EnchantIngredientMatcher(Func<IModifLogger> loggerFactory) => _loggerFactory = loggerFactory;

        public bool TryGetConsumedItems(CompatibleIngredient compatibleIngredient, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems, out List<Item> preservedItems)
        {
            consumedItems = default;
            preservedItems = default;
            //Let the base code figure out the consumed items list
            return false;
        }

        public bool TryGetConsumedStaticItems(CompatibleIngredient compatibleIngredient, Guid staticIngredientID, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems, out List<Item> preservedItems)
        {
            if (EnchantingSettings.PedestalCorelationID == staticIngredientID
                || EnchantingSettings.EquipmentCorelationID == staticIngredientID)
            {
                //Don't consume the enchanting pedestal or equipment piece.
                consumedItems = new List<KeyValuePair<string, int>>();
                preservedItems = compatibleIngredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems").ToList();
                return true;
            }
            //for the other static, just let the base game consume code figure it out
            consumedItems = default;
            preservedItems = default;
            return false;
        }

        public bool TryMatchRecipeStep(CompatibleIngredient potentialIngredient, RecipeIngredient recipeStep, out bool isMatch)
        {
            isMatch = false;
            if (!(recipeStep is CustomRecipeIngredient customIngredient) || customIngredient.CustomRecipeIngredientID != EnchantingSettings.EquipmentCorelationID)
                return false;

            if (!(ParentCraftingMenu.IngredientCraftData.MatchIngredientsRecipe is EnchantRecipe recipe))
                return false;

            var ownedItems = potentialIngredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");

            //exclude non equipped equipment
            if (!ownedItems.Any(i => i is Equipment equipment && equipment.IsEquipped))
            {
#if DEBUG
                Logger.LogTrace($"EnchantIngredientMatcher::TryMatchRecipeStep: potentialIngredient {potentialIngredient.ItemID} - {potentialIngredient.ItemPrefab.DisplayName} " +
                    $"was an Equipment type, but was not equipped.");
#endif
                isMatch = false;
                return true;
            }

            isMatch = recipe.BaseEnchantmentRecipe.CompatibleEquipments.Match(potentialIngredient.ItemPrefab);
#if DEBUG
            Logger.LogTrace($"EnchantIngredientMatcher::TryMatchRecipeStep: compatibleIngredient {potentialIngredient.ItemID} - {potentialIngredient.ItemPrefab.DisplayName} " +
                    $"was {(isMatch ? "a" : "not a")} match for recipe step.");
#endif
            return true;
        }
    }
}
