using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Transmorph.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Transmog
{
    public class IngredientMatcher : ICompatibleIngredientMatcher
    {
		private readonly Func<IModifLogger> _loggerFactory;
		private IModifLogger Logger => _loggerFactory.Invoke();

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

			var tagType = tag.GetTagType();

			var firstItem = ownedItems.FirstOrDefault();
			//If armor, match on equip slot
			if (tagType == Tag.TagTypes.Armor && tag.TryGetEquipmentSlot(out var slot))
            {
				Logger.LogTrace($"{nameof(IngredientMatcher)}::{nameof(MatchRecipeStep)}() Potential Ingredient ItemID: {potentialIngredient.ItemID}. Filter is AddGenericIngredient and TagType is Armor. Matching on " +
					$"Equipment Slot type {slot}.  Potential Ingredient Type: {firstItem?.GetType()}, Name: {firstItem?.DisplayName}, Slot: {(firstItem as Armor)?.EquipSlot}.");
				return ownedItems.Any(i => i is Armor armor && armor.EquipSlot == slot);
			}

			//If weapon, match on weapon type
			if (tagType == Tag.TagTypes.Weapons && tag.TryGetWeaponType(out var weaponType))
			{
				Logger.LogTrace($"{nameof(IngredientMatcher)}::{nameof(MatchRecipeStep)}() Potential Ingredient ItemID: {potentialIngredient.ItemID}. Filter is AddGenericIngredient and TagType is Weapons. Matching on " +
					$"Weapon type {weaponType}.  Potential Ingredient Type: {firstItem?.GetType()}, Name: {firstItem?.DisplayName}, Slot: {(firstItem as Weapon)?.Type}.");
				return ownedItems.Any(i => i is Weapon wep && wep.Type == weaponType);
			}

			return false;
		}
    }
}
