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
    internal class ArmorMatcher : ITransmogMatcher
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public ArmorMatcher(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public bool IsRecipeTag(Tag recipeTag) => recipeTag.GetTagType() == Tag.TagTypes.Armor && recipeTag.TryGetEquipmentSlot(out _);

        public bool IsMatch<T>(T recipe, Tag recipeTag, int ingredientItemID, IEnumerable<Item> ingredientItems) where T : CustomRecipe
        {
            if (!(recipe is TransmogArmorRecipe armorRecipe))
                throw new ArgumentException($"Recipe must be a TransmogArmorRecipe type. Type was {recipe.GetType()}", "recipe");

            if (!recipeTag.TryGetEquipmentSlot(out var slot))
                return false;

            var firstItem = ingredientItems.FirstOrDefault();

            var isMatch = ingredientItems.Any(i => i is Armor armor && armor.EquipSlot == slot
                                            && !((UID)armor.UID).IsTransmogrified()
                                            && armor.IsEquipped
                                            && (armorRecipe == null || i.ItemID != armorRecipe.VisualItemID));

            Logger.LogTrace($"{nameof(ArmorMatcher)}::{nameof(IsMatch)}<T>: Potential Ingredient ItemID: {ingredientItemID} is {(isMatch ? "" : "NOT ")}a match. " +
                $"Filter is AddGenericIngredient and TagType is {recipeTag.GetTagType()}. Matching on " +
                $"Equipment Slot type {slot}.  Potential Ingredient Type: {firstItem?.GetType()}, Name: {firstItem?.DisplayName}, Slot: {(firstItem as Armor)?.EquipSlot}, and any Non Transmog Owned Item UID.");

            return isMatch;
        }

    }
}
