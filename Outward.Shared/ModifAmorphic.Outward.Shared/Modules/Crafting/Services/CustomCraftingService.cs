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

        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public CustomCraftingService(Func<IModifLogger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            CraftingMenuPatches.GenerateResultOverride += TryGenerateResultOverride;
            CraftingMenuPatches.RefreshAvailableIngredientsOverridden += RefreshAvailableIngredientsOverride;
            CraftingMenuPatches.CraftingDoneBefore += StashIngredientEnchantData;
        }

        private void StashIngredientEnchantData(CustomCraftingMenu menu)
        {
            var compatibles = menu.GetSelectedIngredients();
            var consumedUids = compatibles.SelectMany(c => c.GetConsumedItems(false, out _))
                                        .Select(c => c.Key);
            menu.IngredientEnchantData.Clear();
            foreach (var uid in consumedUids)
            {
                var original = ItemManager.Instance.GetItem(uid);

                if (original is Equipment origEq && origEq.IsEnchanted)
                {
                    var enchantData = origEq.GetEnchantmentData();
                    Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(StashIngredientEnchantData)}(): " +
                        $"Original item {origEq.ItemID} - '{origEq.DisplayName}' ({origEq.UID}) contained {origEq.ActiveEnchantmentIDs.Count} enchantments. Stashing Enchantment data.\n" +
                        $"\tData: {enchantData}");
                    menu.IngredientEnchantData.AddOrUpdate(origEq.ItemID, enchantData);
                }
            }
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
            
            dynamicResult?.SetEnchantData(craftingMenu.IngredientEnchantData);

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
            craftingMenu.IngredientEnchantData.Clear();

            //TODO: handle when nothing is crafted. Failure isn't really an option currenly. Returning false
            //will cause the current result (not dynamic) to be generated.
            return true;
        }

        private bool RefreshAvailableIngredientsOverride(CustomCraftingMenu craftingMenu)
        {
            var availableIngredients = craftingMenu.GetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients");
            availableIngredients.Values.ForEach(i => i.Clear());

            craftingMenu.LocalCharacter.Inventory.InventoryIngredients(craftingMenu.InventoryFilterTag, ref availableIngredients);

            Logger.LogDebug($"{craftingMenu.GetType()}::{nameof(RefreshAvailableIngredientsOverride)}(): " +
                        $"Retrieved {availableIngredients.Count} items from character inventory for RecipeCraftingType {craftingMenu.GetRecipeCraftingType()} and Tag {craftingMenu.InventoryFilterTag}.");

            //If a custom ingredient matcher is found, rebuild the list with CustomCompatibleIngredients with the matcher injected
            ICompatibleIngredientMatcher matcher;
            if (_ingredientMatchers.TryGetValue(craftingMenu.GetType(), out matcher))
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
            if (craftingMenu.IncludeEnchantedIngredients)
            {
                AddEnchantingIngredients(craftingMenu.LocalCharacter.Inventory, craftingMenu.InventoryFilterTag, matcher, ref availableIngredients);
            }
            craftingMenu.SetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients", availableIngredients);

            return true;
        }
        private void AddEnchantingIngredients(CharacterInventory inventory, Tag filterTag, ICompatibleIngredientMatcher matcher, ref DictionaryExt<int, CompatibleIngredient> ingredients)
        {
            var allItems = inventory.Pouch.GetContainedItems();
            if (inventory.HasABag)
                allItems.AddRange(inventory.EquippedBag.Container.GetContainedItems());

            foreach (var item in allItems)
            {
                if (item.HasTag(filterTag) && !(item is WaterContainer) && item.IsEnchanted)
                {
                    if (ingredients.ContainsKey(item.ItemID))
                    {
                        ingredients[item.ItemID].AddOwnedItem(item);
                        continue;
                    }
                    CompatibleIngredient compatible = matcher == null ?
                        new CompatibleIngredient(item.ItemID) : new CustomCompatibleIngredient(item.ItemID, matcher, _loggerFactory);
                    compatible.AddOwnedItem(item);
                    ingredients.Add(compatible.ItemID, compatible);
                }
            }
        }
        public void AddOrUpdateCrafter<T>(ICustomCrafter customCrafter)  where T : CustomCraftingMenu =>
            _customCrafters.AddOrUpdate(typeof(T), customCrafter, (k, v) => v = customCrafter);

        public ICompatibleIngredientMatcher AddOrUpdateCompatibleIngredientMatcher<T>(ICompatibleIngredientMatcher matcher) =>
            _ingredientMatchers.AddOrUpdate(typeof(T), matcher, (k, v) => v = matcher);

        public bool TryGetCompatibleIngredientMatcher<T>(out ICompatibleIngredientMatcher matcher) =>
            _ingredientMatchers.TryGetValue(typeof(T), out matcher);
    }
}
