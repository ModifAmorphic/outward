using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients
{
    internal class CustomCompatibleIngredient : CompatibleIngredient
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly ICompatibleIngredientMatcher _ingredientMatcher;

        public CustomCompatibleIngredient(int itemID, ICompatibleIngredientMatcher ingredientMatcher, Func<IModifLogger> loggerFactory) : base(itemID) =>
            (_ingredientMatcher, _loggerFactory) = (ingredientMatcher, loggerFactory);

        public bool MatchRecipeStepOverride(RecipeIngredient recipeIngredient, out bool isMatchResult)
        {
            if (_ingredientMatcher != null)
            {
                isMatchResult = _ingredientMatcher.MatchRecipeStep(this, recipeIngredient);
                return true;
            }
            isMatchResult = false;
            return false;
        }

        public void SetOwnedItems(List<Item> ownedItems)
        {
            this.SetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems", ownedItems);
        }

        public void CaptureConsumedItems(IList<KeyValuePair<string, int>> consumedItems)
        {
            if (_ingredientMatcher != null)
            {
                StashIngredientEnchantData(consumedItems);

                var menuCraftData = _ingredientMatcher.ParentCraftingMenu.IngredientCraftData;
                Dictionary<string, int> itemQuantities;
                if (!menuCraftData.ConsumedItems.TryGetValue(ItemID, out itemQuantities))
                {
                    itemQuantities = new Dictionary<string, int>();
                    menuCraftData.ConsumedItems.Add(ItemID, itemQuantities);
                    Logger.LogDebug($"{nameof(CustomCompatibleIngredient)}::{nameof(CaptureConsumedItems)}: " +
                        $"Added new Dictionary for ItemID {ItemID}.");
                }

                foreach (var kvp in consumedItems)
                {
                    Logger.LogDebug($"{nameof(CustomCompatibleIngredient)}::{nameof(CaptureConsumedItems)}: " +
                        $"Capturing soon to be consumed item {kvp.Value} ({kvp.Key}).");
                    itemQuantities.Add(kvp.Key, kvp.Value);
                }
            }
        }
        private void StashIngredientEnchantData(IList<KeyValuePair<string, int>> consumedItems)
        {
            var compatibles = _ingredientMatcher.ParentCraftingMenu.GetSelectedIngredients();
            //_ingredientMatcher.ParentCraftingMenu.IngredientCraftData.IngredientEnchantData.Clear();
            foreach (var kvp in consumedItems)
            {
                var original = ItemManager.Instance.GetItem(kvp.Key);

                if (original is Equipment origEq && origEq.IsEnchanted)
                {
                    var enchantData = origEq.GetEnchantmentData();
                    Logger.LogDebug($"{nameof(CustomCompatibleIngredient)}::{nameof(StashIngredientEnchantData)}(): " +
                        $"Original item {origEq.ItemID} - '{origEq.DisplayName}' ({origEq.UID}) contained {origEq.ActiveEnchantmentIDs.Count} enchantments. Stashing Enchantment data.\n" +
                        $"\tData: {enchantData}");
                    _ingredientMatcher.ParentCraftingMenu.IngredientCraftData.IngredientEnchantData.AddOrUpdate(origEq.ItemID, enchantData);
                }
            }
        }
    }
}
