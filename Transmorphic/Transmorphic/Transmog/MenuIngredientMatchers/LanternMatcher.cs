using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using ModifAmorphic.Outward.Transmorphic.Settings;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Transmog.MenuIngredientMatchers
{
    internal class LanternMatcher : ITransmogMatcher
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public LanternMatcher(Func<IModifLogger> loggerFactory) => (_loggerFactory) = (loggerFactory);

        public bool IsRecipeTag(Tag recipeTag) => recipeTag.GetTagType() == Tag.TagTypes.Custom && recipeTag == ItemTags.LanternTag;

        public bool IsMatch<T>(T recipe, Tag recipeTag, int ingredientItemID, IEnumerable<Item> ingredientItems) where T : CustomRecipe
        {
            if (!(recipe is TransmogLanternRecipe lanternRecipe))
                throw new ArgumentException($"Recipe must be a TransmogLanternRecipe type. Type was {recipe.GetType()}", "recipe");

            var firstItem = ingredientItems.FirstOrDefault();

            var isMatch = ingredientItems.Any(i => i is Equipment equip
                                            && !((UID)equip.UID).IsTransmogrified()
                                            && equip.IsEquipped
                                            && equip.HasTag(ItemTags.LanternTag)
                                            && (lanternRecipe == null || i.ItemID != lanternRecipe.VisualItemID));
            Logger.LogTrace($"{nameof(LanternMatcher)}::{nameof(IsMatch)}<> Potential Ingredient ItemID: {ingredientItemID} is {(isMatch ? "" : "NOT ")}a match. " +
                $"Filter is AddGenericIngredient and TagType is '{ItemTags.LanternTag}'. Matching on Potential Ingredient Type: {firstItem?.GetType()}, " +
                $"Name: {firstItem?.DisplayName}, Slot: {(firstItem as Equipment)?.EquipSlot}," +
                $" and any Non Transmog Owned Item UID.");

            return isMatch;
        }

    }
}
