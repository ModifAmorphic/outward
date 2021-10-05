using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients
{
    internal class CustomCompatibleIngredient : CompatibleIngredient
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly ICompatibleIngredientMatcher _ingredientMatcher;

        public CustomCompatibleIngredient(int itemID, ICompatibleIngredientMatcher ingredientMatcher, Func<IModifLogger> loggerFactory) : base(itemID) =>
            (_ingredientMatcher, _loggerFactory) = (ingredientMatcher, loggerFactory);

        public bool MatchRecipeStepOverride(RecipeIngredient recipeIngredient, out bool isMatchResult)
        {
            if (_ingredientMatcher != null)
            {
                isMatchResult = _ingredientMatcher.MatchRecipeStep(this, recipeIngredient);
                return true;
            }
            isMatchResult = false;
            return false;
        }

        public void SetOwnedItems(List<Item> ownedItems)
        {
            this.SetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems", ownedItems);
        }
    }
}
