using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
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

        public EnchantResultService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients)
        {
            Logger.LogDebug($"{nameof(EnchantResultService)}::{nameof(CalculateResult)}: Calculating Result for " +
                $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. " +
                $"recipe is EnchantRecipe? {recipe is EnchantRecipe}");
            if (!(recipe is EnchantRecipe enchantRecipe))
                return;

            var ings = ingredients.ToList();
            var equipment = ings.Last().ItemPrefab;

            if (equipment == null)
            {
                Logger.LogDebug($"{nameof(EnchantResultService)}::{nameof(CalculateResult)}: Failed to Calculate result for " +
                    $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. No Equipment ingredient found in list of {ingredients.Count()} ingredients.");
                craftingResult.SetDynamicItemID(-1);
                return;
            }

            craftingResult.SetDynamicItemID(equipment.ItemID);
            var result = craftingResult.DynamicRefItem;
            Logger.LogDebug($"{nameof(EnchantResultService)}::{nameof(CalculateResult)}: Calculated Result " +
                $"is a {result?.DisplayName} ({craftingResult.ItemID}) Enchanted with {enchantRecipe.RecipeName}.");
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
