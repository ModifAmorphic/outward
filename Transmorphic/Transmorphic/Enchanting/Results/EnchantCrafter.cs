﻿using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Results
{
    class EnchantCrafter : ICustomCrafter
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly Action<CharacterInventory, Equipment> _reEquip;

        public EnchantCrafter(Action<CharacterInventory, Equipment> reEquip,  Func<IModifLogger> loggerFactory) => (_reEquip, _loggerFactory) = (reEquip, loggerFactory);

        public bool TryCraftItem(Recipe recipe, ItemReferenceQuantity recipeResult, out Item item, out bool tryEquipItem)
        {
            if (!(recipe is EnchantRecipe enchantRecipe) 
                || !(recipeResult is DynamicCraftingResult dynamicResult)
                || dynamicResult.DynamicItemID == -1)
            {
                Logger.LogError($"{nameof(EnchantCrafter)}::{nameof(TryCraftItem)}(): " +
                        $"Could not craft item. Either recipe was not a EnchantRecipe, result was not a Dynamic result or DynamicItemID was not set. " +
                        $"recipe is EnchantRecipe? {recipe is EnchantRecipe}. " +
                        $"recipeResult is DynamicCraftingResult? {recipeResult is DynamicCraftingResult}. " +
                        $"DynamicItemID: {(recipeResult as DynamicCraftingResult)?.DynamicItemID}");
                item = null;
                tryEquipItem = false;
                return false;
            }

            TryCraftEnchant(enchantRecipe, dynamicResult, out item);
            tryEquipItem = true;
            return true;
        }

        private bool TryCraftEnchant(EnchantRecipe recipe, DynamicCraftingResult dynamicResult, out Item item)
        {
            var equipment = (Equipment)dynamicResult.IngredientCraftData.PreservedItems
                .FirstOrDefault(i => i is Equipment && dynamicResult.DynamicItemID == i.ItemID);
            if (equipment == default)
            {
                item = null;
                return false;
            }
            
            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipe.BaseEnchantmentRecipe.ResultID);
            equipment.AddEnchantment(enchantment.PresetID);
            equipment.LoadedVisual.ApplyVisualModifications();
            //so visuals get applied for virgin armor, anything else that changes when enchanted
            //_reEquip(equipment.OwnerCharacter.Inventory, equipment);
            item = equipment;

            Logger.LogInfo($"Applied enchant '{enchantment.Name}' to item {item.ItemID} - '{item.DisplayName}'.");

            return true;
        }
    }
}