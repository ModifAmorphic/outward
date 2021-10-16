using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using ModifAmorphic.Outward.Transmorphic.Settings;
using ModifAmorphic.Outward.Transmorphic.Transmog.MenuIngredientMatchers;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Transmog
{
    internal class MenuIngredientMatcher : ICompatibleIngredientMatcher, IConsumedItemSelector
	{
		private readonly Func<IModifLogger> _loggerFactory;
		private IModifLogger Logger => _loggerFactory.Invoke();

        public CustomCraftingMenu ParentCraftingMenu { get; set; }


		private readonly ITransmogMatcher[] _ingredientMatchers;

		public MenuIngredientMatcher(IEnumerable<ITransmogMatcher> menuIngredientMatchers, Func<IModifLogger> loggerFactory) =>
			(_ingredientMatchers, _loggerFactory) = (menuIngredientMatchers.ToArray(), loggerFactory);
		
		public bool MatchRecipeStep(CompatibleIngredient potentialIngredient, RecipeIngredient ingredientFilter)
        {
			var ownedItems = potentialIngredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");

			if (ownedItems == null
				|| !ownedItems.Any()
				|| !Enum.IsDefined(typeof(RecipeIngredient.ActionTypes), ingredientFilter.ActionType))
			{
				return false;
			}

			//Specific itemID check.
			if (ingredientFilter.ActionType == RecipeIngredient.ActionTypes.AddSpecificIngredient)
			{
				Logger.LogTrace($"{nameof(MenuIngredientMatcher)}::{nameof(MatchRecipeStep)}() Specified ingredient filter is type AddSpecificIngredient. Matching on " +
					$"ItemIDs. Filter ID: {ingredientFilter.AddedIngredient?.ItemID}, Potential Ingredient ID: {potentialIngredient.ItemID}.");
				return ingredientFilter.AddedIngredient != null && 
					ingredientFilter.AddedIngredient.ItemID == potentialIngredient.ItemID;
			}

			var recipeTag = ingredientFilter.AddedIngredientType.Tag;
			if (recipeTag == Tag.None || TransmogSettings.ExcludedItemIDs.Contains(potentialIngredient.ItemID))
				return false;

			if (!(ParentCraftingMenu.IngredientCraftData.MatchIngredientsRecipe is CustomRecipe recipe))
				return false;

			foreach (var matcher in _ingredientMatchers)
			{
				if (matcher.IsRecipeTag(recipeTag))
				{
					return matcher.IsMatch(recipe, recipeTag, potentialIngredient.ItemID, ownedItems);
				}
			}

			return false;
		}

        public bool TryGetConsumedItems(CompatibleIngredient compatibleIngredient, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems)
        {
			var ownedItems = compatibleIngredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");
			consumedItems = new List<KeyValuePair<string, int>>();

			if (ownedItems == null || ownedItems.Count == 0)
				return false;
					
			var recipe = ParentCraftingMenu.GetSelectedRecipe();

			Logger.LogDebug($"{nameof(MenuIngredientMatcher)}::{nameof(TryGetConsumedItems)}() Potential Ingredient ItemID: {compatibleIngredient.ItemID} - {ownedItems[0].DisplayName}. Recipe: {recipe.Name}. " +
				$"useMultipler: {useMultipler}, resultMultiplier: {resultMultiplier}.");

			//Return first Non Transmogrified Item
			if (recipe is TransmogRecipe tmogRecipe)
            {
				var item = ownedItems.FirstOrDefault(i => i is Equipment equip && !((UID)i.UID).IsTransmogrified() && equip.IsEquipped);
				if (item != default)
				{
					Logger.LogDebug($"{nameof(MenuIngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{tmogRecipe.RecipeName}' is a Transmog. Ingredient {compatibleIngredient.ItemID} - {item.DisplayName} ({item.UID}) " +
						$"was selected to be consumed.");
					consumedItems.Add(new KeyValuePair<string, int>(item.UID, item.ItemID));
					return true;
				}
				Logger.LogDebug($"{nameof(MenuIngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{tmogRecipe.Name}' is a Transmog, but none of the owned ingredients are non-transmog'd equipment types. " +
					$"Keeping base game selection for item {compatibleIngredient.ItemID}. First owned item in list was " +
					$"{ownedItems[0].DisplayName} ({ownedItems[0].UID}) with RemainingAmount: {ownedItems[0].RemainingAmount}.");
				return false;
			}

			//Return first Tranmog'd item if found
			if (recipe is TransmogRemoverRecipe removerRecipe)
			{
				var tmoggedItem = ownedItems.FirstOrDefault(i => i is Equipment equip && ((UID)i.UID).IsTransmogrified() && equip.IsEquipped);
				if (tmoggedItem != default)
				{
					Logger.LogDebug($"{nameof(MenuIngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{removerRecipe.Name}' is a RemoverRecipe. Ingredient {compatibleIngredient.ItemID} - {tmoggedItem.DisplayName} ({tmoggedItem.UID}) " +
						$"was selected to be consumed.");
					consumedItems.Add(new KeyValuePair<string, int>(tmoggedItem.UID, tmoggedItem.ItemID));
					return true;
				}
				Logger.LogDebug($"{nameof(MenuIngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{removerRecipe.Name}' is a RemoverRecipe, but none of the owned ingredients are transmog'd. " +
					$"Keeping base game selection for item {compatibleIngredient.ItemID}. First owned item in list was " +
					$"{ownedItems[0].DisplayName} ({ownedItems[0].UID}) with RemainingAmount: {ownedItems[0].RemainingAmount}.");
				return false;
			}

			Logger.LogDebug($"{nameof(MenuIngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{recipe.Name}' is neither a Transmog or Transmog Remover recipe. Keeping base game selection for item " +
					$"{compatibleIngredient.ItemID}. First owned item in list was {ownedItems[0].DisplayName} ({ownedItems[0].UID}) with RemainingAmount: {ownedItems[0].RemainingAmount}.");
			return false;
		}
    }
}
