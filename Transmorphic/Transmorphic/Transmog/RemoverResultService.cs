using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorph.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorph.Transmog
{
    internal class RemoverResultService : IDynamicResultService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public RemoverResultService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients)
        {
            Logger.LogDebug($"{nameof(RemoverResultService)}::{nameof(CalculateResult)}: Calculating Result for " +
                $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}.");
            if (!(recipe is TransmogRemoverRecipe))
                return;

            var removalTarget = ingredients.FirstOrDefault(i => i?.ItemPrefab is Equipment)?.ItemPrefab as Equipment;


            if (removalTarget == null
                || !(removalTarget is Armor || removalTarget is Weapon))
            {
                Logger.LogWarning($"{nameof(RemoverResultService)}::{nameof(CalculateResult)}: Failed to Calculate result for " +
                    $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. No Armor or Weapons found in {ingredients.Count()} ingredients.");
                craftingResult.SetDynamicItemID(-1);
                return;
            }

            craftingResult.SetDynamicItemID(removalTarget.ItemID);
            var resultEquip = craftingResult.DynamicRefItem as Equipment;
            Logger.LogDebug($"{nameof(RemoverResultService)}::{nameof(CalculateResult)}: Calculated Result " +
                $"is a {craftingResult.DynamicItemID} - {resultEquip?.DisplayName} with the transmogrify removed.");

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
