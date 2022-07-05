using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Transmog
{
    internal class BagResultService : IDynamicResultService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public BagResultService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients)
        {
            Logger.LogDebug($"{nameof(BagResultService)}::{nameof(CalculateResult)}: Calculating Result for " +
                $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. " +
                $"recipe is TransmogBagRecipe? {recipe is TransmogBagRecipe}");
            if (!(recipe is TransmogBagRecipe))
                return;

            var bagTarget = ingredients.FirstOrDefault(i => i?.ItemPrefab is Bag)?.ItemPrefab as Bag;
            var visualBag = craftingResult.RefItem as Bag;

            if (bagTarget == null
                || visualBag == null)
            {
                Logger.LogDebug($"{nameof(BagResultService)}::{nameof(CalculateResult)}: Failed to Calculate result for " +
                    $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. No Bag found in list of {ingredients.Count()} ingredients.");
                craftingResult.SetDynamicItemID(-1);
                return;
            }

            craftingResult.SetDynamicItemID(bagTarget.ItemID);
            var resultBag = craftingResult.DynamicRefItem as Bag;
            Logger.LogDebug($"{nameof(BagResultService)}::{nameof(CalculateResult)}: Calculated Result " +
                $"is a {resultBag?.DisplayName} ({craftingResult.ItemID}) transmogrified to look like" +
                $" a {visualBag.DisplayName} ({visualBag.ItemID}).");

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
