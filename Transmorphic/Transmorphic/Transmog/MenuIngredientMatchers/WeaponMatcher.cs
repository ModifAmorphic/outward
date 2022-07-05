using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Transmog.MenuIngredientMatchers
{
    internal class WeaponMatcher : ITransmogMatcher
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public WeaponMatcher(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public bool IsRecipeTag(Tag recipeTag) => recipeTag.GetTagType() == Tag.TagTypes.Weapons && recipeTag.TryGetWeaponType(out _);

        public bool IsMatch<T>(T recipe, Tag recipeTag, int ingredientItemID, IEnumerable<Item> ingredientItems) where T : CustomRecipe
        {
            if (!(recipe is TransmogWeaponRecipe weaponRecipe))
                throw new ArgumentException($"Recipe must be a TransmogWeaponRecipe type. Type was {recipe.GetType()}", "recipe");

            if (!recipeTag.TryGetWeaponType(out var weaponType))
                return false;

            var firstItem = ingredientItems.FirstOrDefault();

            var isMatch = ingredientItems.Any(i => i is Weapon wep && wep.Type == weaponType
                                            && !((UID)wep.UID).IsTransmogrified()
                                            && wep.IsEquipped
                                            && (weaponRecipe == null || i.ItemID != weaponRecipe.VisualItemID));
            Logger.LogTrace($"{nameof(WeaponMatcher)}::{nameof(IsMatch)}<> Potential Ingredient ItemID: {ingredientItemID} is {(isMatch ? "" : "NOT ")}a match. " +
                $"Filter is AddGenericIngredient and Tag is {recipeTag}. Matching on " +
                $"Weapon type {weaponType}.  Potential Ingredient Type: {firstItem?.GetType()}, Name: {firstItem?.DisplayName}, Slot: {(firstItem as Weapon)?.Type}, and any Non Transmog Owned Item UID.");
            return isMatch;
        }

    }
}
