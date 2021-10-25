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
    internal class BagMatcher : ITransmogMatcher
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public BagMatcher(Func<IModifLogger> loggerFactory) => (_loggerFactory) = (loggerFactory);

        public bool IsRecipeTag(Tag recipeTag) => recipeTag.GetTagType() == Tag.TagTypes.Custom && recipeTag == TransmogSettings.BackpackTag;

        public bool IsMatch<T>(T recipe, Tag recipeTag, int ingredientItemID, IEnumerable<Item> ingredientItems) where T : CustomRecipe
        {
            if (!(recipe is TransmogBagRecipe bagRecipe))
                throw new ArgumentException($"Recipe must be a TransmogBagRecipe type. Type was {recipe.GetType()}", "recipe");

            var firstItem = ingredientItems.FirstOrDefault();

            var isMatch = ingredientItems.Any(i => i is Bag bag
                                            && !((UID)bag.UID).IsTransmogrified()
                                            && bag.IsEquipped
                                            && bag.ContainedSilver < 1 && !bag.Container.GetContainedItems().Any()
                                            && (bagRecipe == null || i.ItemID != bagRecipe.VisualItemID));
            Logger.LogTrace($"{nameof(BagMatcher)}::{nameof(IsMatch)}<> Potential Ingredient ItemID: {ingredientItemID} is {(isMatch ? "" : "NOT ")}a match. " +
                $"Filter is AddGenericIngredient and TagType is '{ TransmogSettings.BackpackTag}'. " +
                $"Matching on Potential Ingredient Type: {firstItem?.GetType()}, Name: {firstItem?.DisplayName}, Slot: {(firstItem as Bag)?.EquipSlot}, and any Non Transmog Owned Item UID.");

            return isMatch;
        }

    }
}
