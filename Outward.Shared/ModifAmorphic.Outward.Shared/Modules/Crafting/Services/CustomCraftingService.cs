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
    /// <summary>
    /// TODO: This should be split at least in half. Half for filtering ingredients, 
    /// other half for actual crafting
    /// </summary>
    internal class CustomCraftingService
    {
        private readonly ConcurrentDictionary<Type, ICustomCrafter> _customCrafters =
           new ConcurrentDictionary<Type, ICustomCrafter>();

        private readonly ConcurrentDictionary<Type, MenuIngredientFilters> _ingredientFilters =
            new ConcurrentDictionary<Type, MenuIngredientFilters>();

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

            var ingredientFilter = GetOrAddIngredientFilter(craftingMenu);

            var availableIngredients = craftingMenu.GetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients");
            int baseCount = availableIngredients.Count;
            Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): " +
                        $"Base CraftingMenu RefreshAvailableIngredients() added {baseCount} available ingredients from character inventory for RecipeCraftingType" +
                        $" {craftingMenu.GetRecipeCraftingType()} and Tag {ingredientFilter.BaseInventoryFilterTag}" +
                        $" for CustomCraftingMenu type {craftingMenu.GetType()}.");

            if (TryGetAnyInjector(craftingMenu, out var matcher, out var itemSelector) && availableIngredients.Count > 0)
            {
                var tmpIngrds = new DictionaryExt<int, CompatibleIngredient>();
                bool isNewCustomIngredients = false;
                for (int i = 0; i < availableIngredients.Keys?.Count; i++)
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
#if DEBUG
                    var logItems = availableIngredients[availableIngredients.Keys[i]].GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");
                    foreach (var item in logItems)
                        Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): {availableIngredients.Keys[i]} - " +
                            $"{item.Name} ({item.UID}). IsEnchanted: {item.IsEnchanted}");
#endif
                }
                //Only set the dictionary if a new CustomCompatibleIngredient was created and added. Otherwise, stick with
                //the original dictionary values.
                if (isNewCustomIngredients)
                    availableIngredients = tmpIngrds;
            }

            if (ingredientFilter.AdditionalInventoryIngredientFilter != null)
            {
                var charInventory = craftingMenu.LocalCharacter.Inventory;
                AddInventoryIngredients(charInventory.Pouch.GetContainedItems(),
                    ingredientFilter.AdditionalInventoryIngredientFilter, matcher, itemSelector, ref availableIngredients);
                if (charInventory.HasABag)
                    AddInventoryIngredients(charInventory.EquippedBag.Container.GetContainedItems(),
                        ingredientFilter.AdditionalInventoryIngredientFilter, matcher, itemSelector, ref availableIngredients);
            }

            if (ingredientFilter.EquippedIngredientFilter != null)
            {
                AddEquippedIngredients(craftingMenu.LocalCharacter.Inventory.Equipment,
                        ingredientFilter.EquippedIngredientFilter,
                        matcher, itemSelector, ref availableIngredients);
            }
            
            craftingMenu.SetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients", availableIngredients);

            Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): " +
                        $"Added {availableIngredients.Count - baseCount} additional items to the collection of available Ingredients for" +
                        $" for menu {craftingMenu.GetType()}, crafting type {craftingMenu.GetRecipeCraftingType()} and Tag {ingredientFilter.BaseInventoryFilterTag}." +
                        $" Original amount of available ingredients was {baseCount}. New amount is {availableIngredients.Count}");

        }
        private MenuIngredientFilters GetOrAddIngredientFilter(CustomCraftingMenu craftingMenu)
        {
            var menuType = craftingMenu.GetType();
            Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(GetOrAddIngredientFilter)}(): " +
                        $"There are {_ingredientFilters.Count} MenuIngredientFilters total. Trying to get filter for Menu Type {menuType}.");
            if (_ingredientFilters.TryGetValue(menuType, out var ingredientFilter))
            {
                Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(GetOrAddIngredientFilter)}(): " +
                        $"Found a registered MenuIngredientFilters for menu type {menuType}");
                if (!ingredientFilter.BaseInventoryFilterTag.IsSet)
                    ingredientFilter.BaseInventoryFilterTag = TagSourceManager.GetCraftingIngredient(craftingMenu.GetRecipeCraftingType());
                
                return ingredientFilter;
            }

            //If no existing filter found, create a new one with defaults and add it.
            //This way a new filter instance doesn't need to be created every time
            //RefreshAvailableIngredients is called.
            var defaultFilter = new MenuIngredientFilters()
            {
                BaseInventoryFilterTag = TagSourceManager.GetCraftingIngredient(craftingMenu.GetRecipeCraftingType()),
                AdditionalInventoryIngredientFilter = null,
                EquippedIngredientFilter = null
            };
            _ingredientFilters.AddOrUpdate(menuType, defaultFilter, (k, v) => v = defaultFilter);

            Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(GetOrAddIngredientFilter)}(): " +
                        $"Added new MenuIngredientFilters for menu type {menuType} with defaults.");

            return defaultFilter;
        }
        private bool TryGetAnyInjector(CustomCraftingMenu parentMenu, out ICompatibleIngredientMatcher matcher, out IConsumedItemSelector itemSelector)
        {
            var menuType = parentMenu.GetType();
            var result = false;

            if (_ingredientMatchers.TryGetValue(menuType, out matcher))
            {
                Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): " +
                    $"Found ICompatibleIngredientMatcher {matcher?.GetType()} registered for CustomCraftingMenu type {menuType}. " +
                    $"Setting ICompatibleIngredientMatcher's ParentCraftingMenu to {menuType} instance.");
                matcher.ParentCraftingMenu = parentMenu;
                result = true;
            }
            if (_itemSelectors.TryGetValue(menuType, out itemSelector))
            {
                Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(RefreshAvailableIngredients)}(): " +
                    $"Found IConsumedItemSelector {itemSelector?.GetType()} registered for CustomCraftingMenu type {menuType}. " +
                    $"Setting IConsumedItemSelector's ParentCraftingMenu to {menuType} instance.");
                itemSelector.ParentCraftingMenu = parentMenu;
                result = true;
            }

            return result;
        }
        private void AddInventoryIngredients(List<Item> inventoryItems, AvailableIngredientFilter filter, ICompatibleIngredientMatcher matcher, IConsumedItemSelector itemSelector, ref DictionaryExt<int, CompatibleIngredient> ingredients)
        {
            int startCount = ingredients.Count;
            foreach (var item in inventoryItems)
            {
                if (ItemPassesFilter(item, filter))
                {
                    if (ingredients.ContainsKey(item.ItemID))
                    {
                        //check the list of owned items to insure the item hasn't already been added by the earlier inventory scan.
                        var ownedItems = ingredients[item.ItemID].GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");
                        if (!ownedItems.Any(i => i.UID == item.UID))
                        {
                            ingredients[item.ItemID].AddOwnedItem(item);
                            Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(AddInventoryIngredients)}(): Added {item.ItemID} - " +
                                $"{item.Name} ({item.UID}). IsEnchanted: {item.IsEnchanted}");
                        }
                        continue;
                    }
                    CompatibleIngredient compatible = matcher == null ?
                        new CompatibleIngredient(item.ItemID) : new CustomCompatibleIngredient(item.ItemID, matcher, itemSelector, _loggerFactory);
                    compatible.AddOwnedItem(item);
                    ingredients.Add(compatible.ItemID, compatible);
                    Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(AddInventoryIngredients)}(): New {item.ItemID} - " +
                        $"{item.Name} ({item.UID}). IsEnchanted: {item.IsEnchanted}");
                }
            }
            Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(AddInventoryIngredients)}(): " +
                        $"Added {ingredients.Count - startCount} additional items from character's inventory to the collection of available Ingredients." +
                        $" Original amount of available ingredients was {startCount}. New amount is {ingredients.Count - startCount}");
        }
        private bool ItemPassesFilter(Item item, AvailableIngredientFilter filter)
        {
            //Tag check
            if (filter.InventoryFilterTag.IsSet && !item.HasTag(filter.InventoryFilterTag))
            {
                Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(ItemPassesFilter)}: Item {item.ItemID} - " +
                        $"{item.Name} ({item.UID}) failed item.HasTag(InventoryFilterTag) check.  Filter tag was {filter.InventoryFilterTag}.");
                return false;
            }
            //Enchantment Checks
            if (filter.EnchantFilter == AvailableIngredientFilter.EnchantFilters.ExcludeEnchanted && item.IsEnchanted)
            {
                Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(ItemPassesFilter)}: Item {item.ItemID} - " +
                        $"{item.Name} ({item.UID}) failed EnchantFilters.ExcludeEnchanted check.  Item.IsEnchanted result was {item.IsEnchanted}.");
                return false;
            }
            if (filter.EnchantFilter == AvailableIngredientFilter.EnchantFilters.OnlyEnchanted && !item.IsEnchanted)
            {
                Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(ItemPassesFilter)}: Item {item.ItemID} - " +
                        $"{item.Name} ({item.UID}) failed EnchantFilters.OnlyEnchanted check.  Item.IsEnchanted result was {item.IsEnchanted}.");
                return false;
            }

            //Type Filtering
            var itemType = item.GetType();
            if (filter.ItemTypes != null && filter.ItemTypes.Count > 0 && !filter.ItemTypes.Any(t => itemType.IsSubclassOf(t) || itemType == t))
            {
                Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(ItemPassesFilter)}: Item {item.ItemID} - " +
                        $"{item.Name} ({item.UID}) failed ItemTypes check.  Item type was {itemType}.");
                return false;
            }

            return true;
        }
        private void AddEquippedIngredients(CharacterEquipment equipment, AvailableIngredientFilter filter, ICompatibleIngredientMatcher matcher, IConsumedItemSelector itemSelector, ref DictionaryExt<int, CompatibleIngredient> ingredients)
        {
            int startCount = ingredients.Count;
            for (int i = 0; i < equipment.EquipmentSlots.Length; i++)
            {
                if (equipment.EquipmentSlots[i] == null || !equipment.EquipmentSlots[i].HasItemEquipped)
                    continue;

                var item = equipment.EquipmentSlots[i].EquippedItem;

                if (ItemPassesFilter(item, filter))
                {
                    if (ingredients.ContainsKey(item.ItemID))
                    {
                        ingredients[item.ItemID].AddOwnedItem(item);
                        Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(AddEquippedIngredients)}(): Added {item.ItemID} - " +
                            $"{item.Name} ({item.UID}). IsEnchanted: {item.IsEnchanted}");
                        continue;
                    }
                    CompatibleIngredient compatible = matcher == null ?
                        new CompatibleIngredient(item.ItemID) : new CustomCompatibleIngredient(item.ItemID, matcher, itemSelector, _loggerFactory);
                    compatible.AddOwnedItem(item);
                    ingredients.Add(compatible.ItemID, compatible);
                    Logger.LogTrace($"{nameof(CustomCraftingService)}::{nameof(AddEquippedIngredients)}(): New {item.ItemID} - " +
                        $"{item.Name} ({item.UID}). IsEnchanted: {item.IsEnchanted}");
                }
            }
            Logger.LogDebug($"{nameof(CustomCraftingService)}::{nameof(AddInventoryIngredients)}(): " +
                        $"Added {ingredients.Count - startCount} additional items from character's inventory to the collection of available Ingredients." +
                        $" Original amount of available ingredients was {startCount}. New amount is {ingredients.Count - startCount}");
        }
        public void AddOrUpdateCrafter<T>(ICustomCrafter customCrafter)  where T : CustomCraftingMenu =>
            _customCrafters.AddOrUpdate(typeof(T), customCrafter, (k, v) => v = customCrafter);

        public MenuIngredientFilters AddOrUpdateIngredientFilter<T>(MenuIngredientFilters filter) =>
            _ingredientFilters.AddOrUpdate(typeof(T), filter, (k, v) => v = filter);
        public bool TryGetIngredientFilter<T>(out MenuIngredientFilters filter) =>
           _ingredientFilters.TryGetValue(typeof(T), out filter);
        
        public ICompatibleIngredientMatcher AddOrUpdateCompatibleIngredientMatcher<T>(ICompatibleIngredientMatcher matcher) =>
            _ingredientMatchers.AddOrUpdate(typeof(T), matcher, (k, v) => v = matcher);

        public bool TryGetCompatibleIngredientMatcher<T>(out ICompatibleIngredientMatcher matcher) =>
            _ingredientMatchers.TryGetValue(typeof(T), out matcher);

        public IConsumedItemSelector AddOrUpdateConsumedItemSelector<T>(IConsumedItemSelector itemSelector) =>
           _itemSelectors.AddOrUpdate(typeof(T), itemSelector, (k, v) => v = itemSelector);
    }
}
