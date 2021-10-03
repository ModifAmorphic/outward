using ModifAmorphic.Outward.Logging;
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

        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public CustomCraftingService(Func<IModifLogger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            CraftingMenuPatches.GenerateResultOverride += GenerateResultOverride;
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

        public void AddOrUpdateCrafter<T>(ICustomCrafter customCrafter)  where T : CustomCraftingMenu =>
            _customCrafters.AddOrUpdate(typeof(T), customCrafter, (k, v) => v = customCrafter);
    }
}
