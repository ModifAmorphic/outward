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
    internal class WeaponResultService : IDynamicResultService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public WeaponResultService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients)
        {
            Logger.LogDebug($"{nameof(WeaponResultService)}::{nameof(CalculateResult)}: Calculating Result for " +
                $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. " +
                $"recipe is TransmogWeaponRecipe? {recipe is TransmogWeaponRecipe}");
            if (!(recipe is TransmogWeaponRecipe weaponRecipe))
                return;

            
            var weaponTarget = ingredients?.FirstOrDefault(i => i?.ItemPrefab is Weapon)?.ItemPrefab as Weapon;
            var visualWeapon = craftingResult.RefItem as Weapon;

            if (weaponTarget == null 
                || visualWeapon == null
                || weaponTarget.Type != visualWeapon.Type)
            {
                Logger.LogWarning($"{nameof(WeaponResultService)}::{nameof(CalculateResult)}: Failed to Calculate result for " +
                    $"Dynamic Result ItemID {craftingResult.ItemID} and recipe {recipe.Name}. No Weapon type '{weaponTarget?.Type}' found in list of {ingredients.Count()} ingredients.");
                craftingResult.SetDynamicItemID(-1);
                return;
            }
            craftingResult.SetDynamicItemID(weaponTarget.ItemID);
            var resultWeapon = craftingResult.DynamicRefItem as Weapon;
            Logger.LogDebug($"{nameof(WeaponResultService)}::{nameof(CalculateResult)}: Calculated Result " +
                $"is a {resultWeapon?.DisplayName} ({craftingResult.ItemID}) {resultWeapon?.Type} transmogrified to look like" +
                $" a {visualWeapon.DisplayName} ({visualWeapon.ItemID}) {visualWeapon.Type}.");
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
