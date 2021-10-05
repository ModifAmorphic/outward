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
    internal class ArmorResultService : IDynamicResultService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public ArmorResultService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients)
        {
            Logger.LogDebug($"{nameof(ArmorResultService)}::{nameof(CalculateResult)}: Calculating Result for " +
                $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. " +
                $"recipe is TransmogArmorRecipe armorRecipe? {recipe is TransmogArmorRecipe}");
            if (!(recipe is TransmogArmorRecipe armorRecipe))
                return;

            var armorTarget = ingredients.FirstOrDefault(i => i?.ItemPrefab is Armor)?.ItemPrefab as Armor;
            var visualArmor = craftingResult.RefItem as Armor;

            if (armorTarget == null
                || visualArmor == null
                || armorTarget.EquipSlot != visualArmor.EquipSlot)
            {
                Logger.LogWarning($"{nameof(ArmorResultService)}::{nameof(CalculateResult)}: Failed to Calculate result for " +
                    $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. No Armor matching equipment slot {armorTarget.EquipSlot} found in list of {ingredients.Count()} ingredients.");
                craftingResult.SetDynamicItemID(-1);
                return;
            }

            craftingResult.SetDynamicItemID(armorTarget.ItemID);
            var resultArmor = craftingResult.DynamicRefItem as Armor;
            Logger.LogDebug($"{nameof(ArmorResultService)}::{nameof(CalculateResult)}: Calculated Result " +
                $"is a {resultArmor?.DisplayName} ({craftingResult.ItemID}) {resultArmor?.EquipSlot} transmogrified to look like" +
                $" a {visualArmor.DisplayName} ({visualArmor.ItemID}) {visualArmor.EquipSlot}.");

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
