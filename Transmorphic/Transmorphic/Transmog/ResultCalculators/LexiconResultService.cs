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
    internal class LexiconResultService : IDynamicResultService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public LexiconResultService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients)
        {
            Logger.LogDebug($"{nameof(LexiconResultService)}::{nameof(CalculateResult)}: Calculating Result for " +
                $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. " +
                $"recipe is TransmogLexiconRecipe? {recipe is TransmogLexiconRecipe}");
            if (!(recipe is TransmogLexiconRecipe))
                return;

            var bookTarget = ingredients.FirstOrDefault(i => i?.ItemPrefab is Equipment 
                                                            && (i?.HasTag(TransmogSettings.LexiconTag) ?? false))?.ItemPrefab as Equipment;
            var visualBook = craftingResult.RefItem as Equipment;

            if (bookTarget == null
                || visualBook == null)
            {
                Logger.LogDebug($"{nameof(LexiconResultService)}::{nameof(CalculateResult)}: Failed to Calculate result for " +
                    $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. No Lexicon found in list of {ingredients.Count()} ingredients.");
                craftingResult.SetDynamicItemID(-1);
                return;
            }

            craftingResult.SetDynamicItemID(bookTarget.ItemID);
            var resultLexicon = craftingResult.DynamicRefItem as Equipment;
            Logger.LogDebug($"{nameof(LexiconResultService)}::{nameof(CalculateResult)}: Calculated Result " +
                $"is a {resultLexicon?.DisplayName} ({craftingResult.ItemID}) transmogrified to look like" +
                $" a {visualBook.DisplayName} ({visualBook.ItemID}).");

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
