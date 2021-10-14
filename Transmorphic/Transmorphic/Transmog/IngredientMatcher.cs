using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Transmorph.Extensions;
using ModifAmorphic.Outward.Transmorph.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Transmog
{
    public class IngredientMatcher : ICompatibleIngredientMatcher, IConsumedItemSelector
	{
		private readonly Func<IModifLogger> _loggerFactory;
		private IModifLogger Logger => _loggerFactory.Invoke();

        public CustomCraftingMenu ParentCraftingMenu { get; set; }

        public IngredientMatcher(Func<IModifLogger> loggerFactory) =>
			(_loggerFactory) = (loggerFactory);
		
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
				Logger.LogTrace($"{nameof(IngredientMatcher)}::{nameof(MatchRecipeStep)}() Specified ingredient filter is type AddSpecificIngredient. Matching on " +
					$"ItemIDs. Filter ID: {ingredientFilter.AddedIngredient?.ItemID}, Potential Ingredient ID: {potentialIngredient.ItemID}.");
				return ingredientFilter.AddedIngredient != null && 
					ingredientFilter.AddedIngredient.ItemID == potentialIngredient.ItemID;
			}

			var tag = ingredientFilter.AddedIngredientType.Tag;
			if (tag == Tag.None)
				return false;

			var tmogRecipe = ParentCraftingMenu.IngredientCraftData.MatchIngredientsRecipe as TransmogRecipe;
			var tagType = tag.GetTagType();

			var firstItem = ownedItems.FirstOrDefault();
			//If armor, match on equip slot
			if (tagType == Tag.TagTypes.Armor && tag.TryGetEquipmentSlot(out var slot))
            {
				Logger.LogTrace($"{nameof(IngredientMatcher)}::{nameof(MatchRecipeStep)}() Potential Ingredient ItemID: {potentialIngredient.ItemID}. Filter is AddGenericIngredient and TagType is Armor. Matching on " +
					$"Equipment Slot type {slot}.  Potential Ingredient Type: {firstItem?.GetType()}, Name: {firstItem?.DisplayName}, Slot: {(firstItem as Armor)?.EquipSlot}, and any Non Transmog Owned Item UID.");
				return ownedItems.Any(i => i is Armor armor && armor.EquipSlot == slot 
											&& !((UID)armor.UID).IsTransmogrified() 
											&& armor.IsEquipped
											&& (tmogRecipe == null || i.ItemID != tmogRecipe.VisualItemID));
			}

			//If weapon, match on weapon type
			if (tagType == Tag.TagTypes.Weapons && tag.TryGetWeaponType(out var weaponType))
			{
				Logger.LogTrace($"{nameof(IngredientMatcher)}::{nameof(MatchRecipeStep)}() Potential Ingredient ItemID: {potentialIngredient.ItemID}. Filter is AddGenericIngredient and TagType is Weapons. Matching on " +
					$"Weapon type {weaponType}.  Potential Ingredient Type: {firstItem?.GetType()}, Name: {firstItem?.DisplayName}, Slot: {(firstItem as Weapon)?.Type}, and any Non Transmog Owned Item UID.");
				return ownedItems.Any(i => i is Weapon wep && wep.Type == weaponType 
											&& !((UID)wep.UID).IsTransmogrified()
											&& wep.IsEquipped
											&& (tmogRecipe == null || i.ItemID != tmogRecipe.VisualItemID));
			}

			if (tagType == Tag.TagTypes.Custom && tag.IsRemoverTag())
            {
				Logger.LogTrace($"{nameof(IngredientMatcher)}::{nameof(MatchRecipeStep)}() Potential Ingredient ItemID: {potentialIngredient.ItemID}. Filter is AddGenericIngredient and TagType is a Custom IsRemoverTag. Matching on " +
					$"any owned Tranmogrified Equipment items (m_ownedItems => i is Equipment and i.IsTransmog()).");
				return ownedItems.Any(i => i is Equipment equip 
											&& ((UID)equip.UID).IsTransmogrified()
											&& equip.IsEquipped);
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

			Logger.LogDebug($"{nameof(IngredientMatcher)}::{nameof(TryGetConsumedItems)}() Potential Ingredient ItemID: {compatibleIngredient.ItemID} - {ownedItems[0].DisplayName}. Recipe: {recipe.Name}. " +
				$"useMultipler: {useMultipler}, resultMultiplier: {resultMultiplier}.");

			//Return first Non Transmogrified Item
			if (recipe is TransmogRecipe tmogRecipe)
            {
				var item = ownedItems.FirstOrDefault(i => i is Equipment equip && !((UID)i.UID).IsTransmogrified() && equip.IsEquipped);
				if (item != default)
				{
					Logger.LogDebug($"{nameof(IngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{tmogRecipe.RecipeName}' is a Transmog. Ingredient {compatibleIngredient.ItemID} - {item.DisplayName} ({item.UID}) " +
						$"was selected to be consumed.");
					consumedItems.Add(new KeyValuePair<string, int>(item.UID, item.ItemID));
					return true;
				}
				Logger.LogDebug($"{nameof(IngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{tmogRecipe.Name}' is a Transmog, but none of the owned ingredients are non-transmog'd equipment types. Keeping base game selection for item " +
					$"{compatibleIngredient.ItemID}. First owned item in list was {ownedItems[0].DisplayName} ({ownedItems[0].UID}) with RemainingAmount: {ownedItems[0].RemainingAmount}.");
			}

			//Return first Tranmog'd item if found
			if (recipe is TransmogRemoverRecipe removerRecipe)
			{
				var tmoggedItem = ownedItems.FirstOrDefault(i => i is Equipment equip && ((UID)i.UID).IsTransmogrified() && equip.IsEquipped);
				if (tmoggedItem != default)
				{
					Logger.LogDebug($"{nameof(IngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{removerRecipe.Name}' is a RemoverRecipe. Ingredient {compatibleIngredient.ItemID} - {tmoggedItem.DisplayName} ({tmoggedItem.UID}) " +
						$"was selected to be consumed.");
					consumedItems.Add(new KeyValuePair<string, int>(tmoggedItem.UID, tmoggedItem.ItemID));
					return true;
				}
				Logger.LogDebug($"{nameof(IngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{removerRecipe.Name}' is a RemoverRecipe, but none of the owned ingredients are transmog'd. Keeping base game selection for item " +
					$"{compatibleIngredient.ItemID}. First owned item in list was {ownedItems[0].DisplayName} ({ownedItems[0].UID}) with RemainingAmount: {ownedItems[0].RemainingAmount}.");
			}

			Logger.LogDebug($"{nameof(IngredientMatcher)}::{nameof(TryGetConsumedItems)}() Recipe '{recipe.Name}' is neither a Transmog or Transmog Remover recipe. Keeping base game selection for item " +
					$"{compatibleIngredient.ItemID}. First owned item in list was {ownedItems[0].DisplayName} ({ownedItems[0].UID}) with RemainingAmount: {ownedItems[0].RemainingAmount}.");
			return false;
		}
    }
}
