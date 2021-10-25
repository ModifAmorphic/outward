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
    internal class RemoverMatcher : ITransmogMatcher
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public RemoverMatcher(Func<IModifLogger> loggerFactory) => (_loggerFactory) = (loggerFactory);

        public bool IsRecipeTag(Tag recipeTag) => recipeTag.GetTagType() == Tag.TagTypes.Custom && recipeTag.IsRemoverTag();

        public bool IsMatch<T>(T recipe, Tag recipeTag, int ingredientItemID, IEnumerable<Item> ingredientItems) where T : CustomRecipe
        {
            if (!(recipe is TransmogRemoverRecipe removerRecipe))
                throw new ArgumentException($"Recipe must be a TransmogRemoverRecipe type. Type was {recipe?.GetType()}", "recipe");

            var isMatch = ingredientItems.Any(i => i is Equipment equip
                                            && ((UID)equip.UID).IsTransmogrified()
                                            && (!(equip is Bag bag) || (bag.ContainedSilver < 1 && !bag.Container.GetContainedItems().Any()))
                                            && equip.IsEquipped);
            Logger.LogTrace($"{nameof(RemoverMatcher)}::{nameof(IsMatch)}<> Potential Ingredient ItemID: {ingredientItemID} is {(isMatch ? "" : "NOT ")}a match. " +
                $"Filter is AddGenericIngredient and TagType is a Custom IsRemoverTag. Matching on " +
                $"any owned Tranmogrified Equipment items (m_ownedItems => i is Equipment and i.IsTransmog()).");

            return isMatch;
        }

    }
}
