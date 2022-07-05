using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Results
{
    internal class EnchantResultService : IDynamicResultService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly EnchantPrefabs _enchantPrefabs;

        public EnchantResultService(EnchantPrefabs enchantPrefabs, Func<IModifLogger> loggerFactory) =>
            (_enchantPrefabs, _loggerFactory) = (enchantPrefabs, loggerFactory);

        public void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients)
        {
            Logger.LogDebug($"{nameof(EnchantResultService)}::{nameof(CalculateResult)}: Calculating Result for " +
                $"ItemID {craftingResult.ItemID} and recipe {recipe.Name}. " +
                $"recipe is EnchantRecipe? {recipe is EnchantRecipe}");
            if (!(recipe is EnchantRecipe enchantRecipe))
                return;

            var lastIngredient = ingredients.ToList().Last();
            
            if (lastIngredient.ItemPrefab == null)
            {
                Logger.LogDebug($"{nameof(EnchantResultService)}::{nameof(CalculateResult)}: Failed to Calculate result for " +
                    $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. No Equipment ingredient found in list of {ingredients.Count()} ingredients.");
                craftingResult.SetDynamicItemID(-1);
                return;
            }

            Equipment resultEquipment;
            if (lastIngredient.ItemPrefab is Equipment equipment)
            {
                Logger.LogDebug($"{nameof(EnchantResultService)}::{nameof(CalculateResult)}: Calculate result for selected ingredient {equipment?.ItemID}");
                resultEquipment = _enchantPrefabs.GenerateEnchantRecipeResult(enchantRecipe.BaseEnchantmentRecipe, equipment.ItemID);
            }
            else
            {
                resultEquipment = _enchantPrefabs.GetOrCreateResultPrefab(enchantRecipe.BaseEnchantmentRecipe);
            }

            craftingResult.SetDynamicItemID(lastIngredient.ItemID);
            var dynamicResult = craftingResult.DynamicRefItem;
            craftingResult.ItemID = resultEquipment.ItemID;
            craftingResult.SetPrivateField<ItemReferenceQuantity, Item>("m_item", resultEquipment);
            
            Logger.LogDebug($"{nameof(EnchantResultService)}::{nameof(CalculateResult)}: Calculated Result " +
                $"is a {craftingResult.RefItem?.DisplayName} ({craftingResult.ItemID}) Enchanted with {enchantRecipe.RecipeName}. " +
                $"Active Enchants: {(craftingResult.RefItem as Equipment)?.ActiveEnchantmentIDs?.Count}");
        }

        public void SetDynamicItemID(DynamicCraftingResult craftingResult, int newItemId, ref int itemID, ref Item item)
        {
            if (newItemId != itemID)
            {
                itemID = newItemId;
                if (!Application.isPlaying && !ResourcesPrefabManager.Instance.Loaded)
                {
                    ResourcesPrefabManager.Instance.Load();
                }
                item = ResourcesPrefabManager.Instance.GetItemPrefab(itemID);
            }
        }

    }
}
