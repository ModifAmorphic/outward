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
    internal class LexiconMatcher : ITransmogMatcher
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public LexiconMatcher(Func<IModifLogger> loggerFactory) => (_loggerFactory) = (loggerFactory);

        public bool IsRecipeTag(Tag recipeTag) => recipeTag.GetTagType() == Tag.TagTypes.Custom && recipeTag == TransmogSettings.LexiconTag;

        public bool IsMatch<T>(T recipe, Tag recipeTag, int ingredientItemID, IEnumerable<Item> ingredientItems) where T : CustomRecipe
        {
            if (!(recipe is TransmogLexiconRecipe lexRecipe))
                throw new ArgumentException($"Recipe must be a TransmogLexiconRecipe type. Type was {recipe.GetType()}", "recipe");

            var firstItem = ingredientItems.FirstOrDefault();

            var isMatch = ingredientItems.Any(i => i is Equipment equip
                                            && !((UID)equip.UID).IsTransmogrified()
                                            && equip.IsEquipped
                                            && equip.HasTag(TransmogSettings.LexiconTag)
                                            && (lexRecipe == null || i.ItemID != lexRecipe.VisualItemID));
            Logger.LogTrace($"{nameof(LexiconMatcher)}::{nameof(IsMatch)}<> Potential Ingredient ItemID: {ingredientItemID} is {(isMatch ? "" : "NOT ")}a match. " +
                $"Filter is AddGenericIngredient and TagType is '{TransmogSettings.LexiconTag}'. Matching on Potential Ingredient Type: {firstItem?.GetType()}, " +
                $"Name: {firstItem?.DisplayName}, Slot: {(firstItem as Equipment)?.EquipSlot}," +
                $" and any Non Transmog Owned Item UID.");

            return isMatch;
        }

    }
}
