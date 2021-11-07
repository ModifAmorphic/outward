using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
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

        private readonly ModifCoroutine _coroutine;
        public EnchantCrafter(ModifCoroutine coroutine, Func<IModifLogger> loggerFactory) => (_coroutine, _loggerFactory) = (coroutine, loggerFactory);

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
            DelayedReEquip(item.OwnerCharacter.Inventory, (Equipment)item);
            tryEquipItem = true;
            return true;
        }

        private bool TryCraftEnchant(EnchantRecipe recipe, DynamicCraftingResult dynamicResult, out Item item)
        {
            var equipment = (Equipment)dynamicResult.IngredientCraftData.PreservedItems
                .FirstOrDefault(i => i is Equipment && dynamicResult.DynamicItemID == i.ItemID);
            if (equipment == default)
            {
                Logger.LogError($"{nameof(EnchantCrafter)}::{nameof(TryCraftEnchant)}(): " +
                        $"Could not enchant item. Unable to locate source equipment itemID {dynamicResult.DynamicItemID} in list of preservered ingredients.");
                item = null;
                return false;
            }

            equipment.OwnerCharacter.Inventory.UnequipItem(equipment, false);

            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipe.BaseEnchantmentRecipe.ResultID);
            equipment.AddEnchantment(enchantment.PresetID);
            //equipment.LoadedVisual.ApplyVisualModifications();
            item = equipment;

            Logger.LogInfo($"Applied enchant '{enchantment.Name}' to item {item.ItemID} - '{item.DisplayName}'.");

            return true;
        }

        private void DelayedReEquip(CharacterInventory inventory, Equipment equipment)
        {
            Func<bool> isUnequipped = () =>
            {
                return !(equipment.IsEquipped && inventory.Equipment.IsHandFree(equipment));
            };
            Action reEquip = () => inventory.EquipItem(equipment);

            _coroutine.StartRoutine(
                _coroutine.InvokeAfter(isUnequipped, reEquip, 5, .1f)
            );
        }
    }
}
