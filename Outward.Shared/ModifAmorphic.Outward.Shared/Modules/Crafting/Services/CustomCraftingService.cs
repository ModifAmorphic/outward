using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Modules.Crafting.Patches;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting.Services
{
    internal class CustomCraftingService
    {
        private readonly ConcurrentDictionary<Type, ICustomCrafter> _customCrafters =
           new ConcurrentDictionary<Type, ICustomCrafter>();

        private readonly ConcurrentDictionary<Type, ICompatibleIngredientMatcher> _ingredientMatchers =
            new ConcurrentDictionary<Type, ICompatibleIngredientMatcher>();

        private readonly ConcurrentDictionary<Type, IConsumedItemSelector> _itemSelectors =
            new ConcurrentDictionary<Type, IConsumedItemSelector>();
        

        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public CustomCraftingService(Func<IModifLogger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            CraftingMenuPatches.GenerateResultOverride += TryGenerateResultOverride;
            CraftingMenuPatches.RefreshAvailableIngredientsAfter += TryRefreshAvailableIngredients;
        }

        private bool TryGenerateResultOverride((CustomCraftingMenu CraftingMenu, ItemReferenceQuantity Result, int ResultMultiplier) arg)
        {
            try
            {
                return GenerateResultOverride(arg.CraftingMenu, arg.Result, arg.ResultMultiplier);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{arg.CraftingMenu?.GetType()}::{nameof(TryGenerateResultOverride)}(): " +
                        $"Error in GenerateResultOverride(). Could not craft item. Ingredients have likely been lost!", ex);
                return true;
            }
        }
        private bool GenerateResultOverride(CustomCraftingMenu craftingMenu, ItemReferenceQuantity result, int resultMultiplier)
        {

            if (result == null
                || ResourcesPrefabManager.Instance.IsWaterItem(result.RefItem.ItemID)
                || !_customCrafters.TryGetValue(craftingMenu.GetType(), out var crafter))
                return false;

            var characterInventory = craftingMenu.LocalCharacter.Inventory;
            int quantity = result.Quantity * resultMultiplier;

            var dynamicResult = result as DynamicCraftingResult;
            
            dynamicResult?.SetIngredientData(craftingMenu.IngredientCraftData);

            var craftedAny = false;
            for (int i = 0; i < quantity; i++)
            {
                Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(GenerateResultOverride)}(): " +
                    $"Trying to craft item from recipe {craftingMenu.GetSelectedRecipe().Name} and result ItemID {result.ItemID};");
                if (crafter.TryCraftItem(craftingMenu.GetSelectedRecipe(), result, out var craftedItem))
                {
                    Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(GenerateResultOverride)}(): " +
                        $"New item '{craftedItem.Name}' crafted from recipe {craftingMenu.GetSelectedRecipe().Name} and result ItemID {result.ItemID};");
                    craftedAny = true;
                    characterInventory.TakeItem(craftedItem, false);
                    characterInventory.NotifyItemTake(craftedItem, 1);
                }
            }

            dynamicResult?.ResetResult();
            craftingMenu.IngredientCraftData.Reset();

            //TODO: handle when nothing is crafted. Failure isn't really an option currenly. Returning false
            //will cause the current result (not dynamic) to be generated.
            return true;
        }

        private void TryRefreshAvailableIngredients(CustomCraftingMenu craftingMenu)
        {
            try
            {
                RefreshAvailableIngredients(craftingMenu);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CustomCraftingService)}::{nameof(TryRefreshAvailableIngredients)}(): " +
                        $"Exception in call {nameof(RefreshAvailableIngredients)} for CustomCraftingMenu type {craftingMenu.GetType()}.", ex);
            }
        }
        private void RefreshAvailableIngredients(CustomCraftingMenu craftingMenu)
        {
            var availableIngredients = craftingMenu.GetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients");
            
            Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): " +
                        $"Retrieved {availableIngredients.Count} items from character inventory for RecipeCraftingType {craftingMenu.GetRecipeCraftingType()} and Tag {craftingMenu.InventoryFilterTag}" +
                        $" for CustomCraftingMenu type {craftingMenu.GetType()}.");

            //If a custom ingredient matcher is found, rebuild the list with CustomCompatibleIngredients with the matcher injected
            _ingredientMatchers.TryGetValue(craftingMenu.GetType(), out var matcher);
            _itemSelectors.TryGetValue(craftingMenu.GetType(), out var itemSelector);

            if (matcher != null || itemSelector != null)
            {
                if (matcher != null)
                    Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): " +
                        $"Found ICompatibleIngredientMatcher {matcher?.GetType()} registered for CustomCraftingMenu type {craftingMenu.GetType()}.");
                if (itemSelector != null)
                    Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): " +
                        $"Found IConsumedItemSelector {itemSelector?.GetType()} registered for CustomCraftingMenu type {craftingMenu.GetType()}.");
                if (matcher != null)
                    matcher.ParentCraftingMenu = craftingMenu;
                if (itemSelector != null)
                    itemSelector.ParentCraftingMenu = craftingMenu;
                var tmpIngrds = new DictionaryExt<int, CompatibleIngredient>();
                bool isNewCustomIngredients = false;
                for (int i = 0; i < availableIngredients.Keys.Count; i++)
                {
                    var ingredient = availableIngredients[availableIngredients.Keys[i]];
                    if (!(ingredient is CustomCompatibleIngredient custIngredient))
                    {
                        isNewCustomIngredients = true;
                        var newCustIngredient = new CustomCompatibleIngredient(ingredient.ItemID, matcher, itemSelector, _loggerFactory);

                        //m_ownedItems gets added to by the earlier InventoryIngredients call, so copy that as well.
                        var ownedItems = ingredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");
                        foreach (var o in ownedItems)
                        {
                            newCustIngredient.AddOwnedItem(o);
                        }
                        tmpIngrds.Add(availableIngredients.Keys[i], newCustIngredient);
                    }
                    else
                    {
                        //add the original ingredient just in case there are some new CustomCompatibleIngredients created
                        tmpIngrds.Add(availableIngredients.Keys[i], custIngredient);
                    }
                    var logItems = availableIngredients[availableIngredients.Keys[i]].GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");
                    foreach (var item in logItems)
                        Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): {availableIngredients.Keys[i]} - " +
                            $"{item.Name} ({item.UID}). IsEnchanted: {item.IsEnchanted}");
                }
                //Only set the dictionary if a new CustomCompatibleIngredient was created and added. Otherwise, stick with
                //the original dictionary values.
                if (isNewCustomIngredients)
                    availableIngredients = tmpIngrds;
            }
            if (craftingMenu.IncludeEnchantedIngredients)
            {
                AddEnchantingIngredients(craftingMenu.LocalCharacter.Inventory.Pouch.GetContainedItems(), 
                    craftingMenu.InventoryFilterTag, matcher, itemSelector, ref availableIngredients);
                AddEnchantingIngredients(craftingMenu.LocalCharacter.Inventory.EquippedBag.Container.GetContainedItems(), 
                    craftingMenu.InventoryFilterTag, matcher, itemSelector, ref availableIngredients);
            }
            craftingMenu.SetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients", availableIngredients);

        }
        private void AddEnchantingIngredients(List<Item> inventoryItems, Tag filterTag, ICompatibleIngredientMatcher matcher, IConsumedItemSelector itemSelector, ref DictionaryExt<int, CompatibleIngredient> ingredients)
        {
            foreach (var item in inventoryItems)
            {
                if (item.HasTag(filterTag) && !(item is WaterContainer) && item.IsEnchanted)
                {
                    if (ingredients.ContainsKey(item.ItemID))
                    {
                        ingredients[item.ItemID].AddOwnedItem(item);
                        Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(AddEnchantingIngredients)}(): Added {item.ItemID} - " +
                            $"{item.Name} ({item.UID}). IsEnchanted: {item.IsEnchanted}");
                        continue;
                    }
                    CompatibleIngredient compatible = matcher == null ?
                        new CompatibleIngredient(item.ItemID) : new CustomCompatibleIngredient(item.ItemID, matcher, itemSelector, _loggerFactory);
                    compatible.AddOwnedItem(item);
                    ingredients.Add(compatible.ItemID, compatible);
                    Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(AddEnchantingIngredients)}(): New {item.ItemID} - " +
                        $"{item.Name} ({item.UID}). IsEnchanted: {item.IsEnchanted}");
                }
            }
        }
        public void AddOrUpdateCrafter<T>(ICustomCrafter customCrafter)  where T : CustomCraftingMenu =>
            _customCrafters.AddOrUpdate(typeof(T), customCrafter, (k, v) => v = customCrafter);

        public ICompatibleIngredientMatcher AddOrUpdateCompatibleIngredientMatcher<T>(ICompatibleIngredientMatcher matcher) =>
            _ingredientMatchers.AddOrUpdate(typeof(T), matcher, (k, v) => v = matcher);

        public bool TryGetCompatibleIngredientMatcher<T>(out ICompatibleIngredientMatcher matcher) =>
            _ingredientMatchers.TryGetValue(typeof(T), out matcher);

        public IConsumedItemSelector AddOrUpdateConsumedItemSelector<T>(IConsumedItemSelector itemSelector) =>
           _itemSelectors.AddOrUpdate(typeof(T), itemSelector, (k, v) => v = itemSelector);
    }
}
