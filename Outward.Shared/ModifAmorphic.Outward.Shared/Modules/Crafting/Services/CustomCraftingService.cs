using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Modules.Crafting.Patches;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting.Services
{
    internal class CustomCraftingService
    {
        private readonly ConcurrentDictionary<Type, ICustomCrafter> _customCrafters =
           new ConcurrentDictionary<Type, ICustomCrafter>();

        private readonly ConcurrentDictionary<Type, ICompatibleIngredientMatcher> _ingredientMatchers =
            new ConcurrentDictionary<Type, ICompatibleIngredientMatcher>();

        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public CustomCraftingService(Func<IModifLogger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            CraftingMenuPatches.GenerateResultOverride += GenerateResultOverride;
            CraftingMenuPatches.RefreshAvailableIngredientsOverridden += RefreshAvailableIngredientsOverride;
        }


        private bool GenerateResultOverride((CraftingMenu CraftingMenu, ItemReferenceQuantity Result, int ResultMultiplier) arg)
        {
            (var craftingMenu, var result, var resultMultiplier) = (arg.CraftingMenu, arg.Result, arg.ResultMultiplier);

            Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(GenerateResultOverride)}(): " +
               $"result == null ? {result == null}; craftingMenu is CustomCraftingMenu? {craftingMenu is CustomCraftingMenu};");

            if (result == null || !(craftingMenu is CustomCraftingMenu customCraftingMenu)
                || ResourcesPrefabManager.Instance.IsWaterItem(result.RefItem.ItemID)
                || !_customCrafters.TryGetValue(customCraftingMenu.GetType(), out var crafter))
                return false;

            var characterInventory = craftingMenu.LocalCharacter.Inventory;
            int quantity = result.Quantity * resultMultiplier;

            var craftedAny = false;
            for (int i = 0; i < quantity; i++)
            {
                Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(GenerateResultOverride)}(): " +
                    $"Trying to craft item from recipe {customCraftingMenu.GetSelectedRecipe().Name} and result ItemID {result.ItemID};");
                if (crafter.TryCraftItem(customCraftingMenu.GetSelectedRecipe(), result, out var craftedItem))
                {
                    Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(GenerateResultOverride)}(): " +
                        $"New item '{craftedItem.Name}' crafted from recipe {customCraftingMenu.GetSelectedRecipe().Name} and result ItemID {result.ItemID};");
                    craftedAny = true;
                    characterInventory.TakeItem(craftedItem, false);
                    characterInventory.NotifyItemTake(craftedItem, 1);
                    if (result is DynamicCraftingResult dynamicResult)
                        dynamicResult.ResetResult();
                }
            }

            return craftedAny;
        }

        private bool RefreshAvailableIngredientsOverride(CustomCraftingMenu craftingMenu)
        {
            var availableIngredients = craftingMenu.GetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients");
            availableIngredients.Values.ForEach(i => i.Clear());

            craftingMenu.LocalCharacter.Inventory.InventoryIngredients(craftingMenu.InventoryFilterTag, ref availableIngredients);

            Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredientsOverride)}(): " +
                        $"Retrieved {availableIngredients.Count} items from character inventory for RecipeCraftingType {craftingMenu.GetRecipeCraftingType()} and Tag {craftingMenu.InventoryFilterTag}.");
            
            //If a custom ingredient matcher is found, rebuild the list with CustomCompatibleIngredients with the matcher injected
            if (_ingredientMatchers.TryGetValue(craftingMenu.GetType(), out var matcher))
            {
                var tmpIngrds = new DictionaryExt<int, CompatibleIngredient>();
                for (int i = 0; i < availableIngredients.Keys.Count; i++)
                {
                    var ingredient = availableIngredients[availableIngredients.Keys[i]];
                    var custIngredient = new CustomCompatibleIngredient(ingredient.ItemID, matcher, _loggerFactory);

                    //m_ownedItems gets added to by the earlier InventoryIngredients call, so copy that as well.
                    var ownedItems = ingredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");
                    foreach (var o in ownedItems)
                    {
                        custIngredient.AddOwnedItem(o);
                    }

                    tmpIngrds.Add(availableIngredients.Keys[i], custIngredient);
                }
                availableIngredients = tmpIngrds;
            }
            craftingMenu.SetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients", availableIngredients);

            return true;
        }

        public void AddOrUpdateCrafter<T>(ICustomCrafter customCrafter)  where T : CustomCraftingMenu =>
            _customCrafters.AddOrUpdate(typeof(T), customCrafter, (k, v) => v = customCrafter);

        public ICompatibleIngredientMatcher AddOrUpdateCompatibleIngredientMatcher<T>(ICompatibleIngredientMatcher matcher) =>
            _ingredientMatchers.AddOrUpdate(typeof(T), matcher, (k, v) => v = matcher);

        public bool TryGetCompatibleIngredientMatcher<T>(out ICompatibleIngredientMatcher matcher) =>
            _ingredientMatchers.TryGetValue(typeof(T), out matcher);
    }
}
