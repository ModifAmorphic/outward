using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Settings;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Transmog
{
    internal class LanternResultService : IDynamicResultService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public LanternResultService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients)
        {
            Logger.LogDebug($"{nameof(LanternResultService)}::{nameof(CalculateResult)}: Calculating Result for " +
                $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. " +
                $"recipe is TransmogLanternRecipe? {recipe is TransmogLanternRecipe}");
            if (!(recipe is TransmogLanternRecipe))
                return;

            var lanternTarget = ingredients.FirstOrDefault(i => i?.ItemPrefab is Equipment 
                                                            && (i?.HasTag(TransmogSettings.LanternTag) ?? false))?.ItemPrefab as Equipment;
            var visualLantern = craftingResult.RefItem as Equipment;

            if (lanternTarget == null
                || visualLantern == null)
            {
                Logger.LogDebug($"{nameof(LanternResultService)}::{nameof(CalculateResult)}: Failed to Calculate result for " +
                    $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. No Lantern found in list of {ingredients.Count()} ingredients.");
                craftingResult.SetDynamicItemID(-1);
                return;
            }

            craftingResult.SetDynamicItemID(lanternTarget.ItemID);
            var resultLantern = craftingResult.DynamicRefItem as Equipment;
            Logger.LogDebug($"{nameof(LanternResultService)}::{nameof(CalculateResult)}: Calculated Result " +
                $"is a {resultLantern?.DisplayName} ({craftingResult.ItemID}) transmogrified to look like" +
                $" a {visualLantern.DisplayName} ({visualLantern.ItemID}).");

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
