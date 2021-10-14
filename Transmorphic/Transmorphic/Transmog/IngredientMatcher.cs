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
				return ownedItems.Any(i => i is Armor armor && armor.EquipSlot == slot && !((UID)armor.UID).IsTransmogrified() 
											&& (tmogRecipe == null || i.ItemID != tmogRecipe.VisualItemID));
			}

			//If weapon, match on weapon type
			if (tagType == Tag.TagTypes.Weapons && tag.TryGetWeaponType(out var weaponType))
			{
				Logger.LogTrace($"{nameof(IngredientMatcher)}::{nameof(MatchRecipeStep)}() Potential Ingredient ItemID: {potentialIngredient.ItemID}. Filter is AddGenericIngredient and TagType is Weapons. Matching on " +
					$"Weapon type {weaponType}.  Potential Ingredient Type: {firstItem?.GetType()}, Name: {firstItem?.DisplayName}, Slot: {(firstItem as Weapon)?.Type}, and any Non Transmog Owned Item UID.");
				return ownedItems.Any(i => i is Weapon wep && wep.Type == weaponType && !((UID)wep.UID).IsTransmogrified()
											&& (tmogRecipe == null || i.ItemID != tmogRecipe.VisualItemID));
			}

			if (tagType == Tag.TagTypes.Custom && tag.IsRemoverTag())
            {
				Logger.LogTrace($"{nameof(IngredientMatcher)}::{nameof(MatchRecipeStep)}() Potential Ingredient ItemID: {potentialIngredient.ItemID}. Filter is AddGenericIngredient and TagType is a Custom IsRemoverTag. Matching on " +
					$"any owned Tranmogrified Equipment items (m_ownedItems => i is Equipment and i.IsTransmog()).");
				return ownedItems.Any(i => i is Equipment equip && ((UID)equip.UID).IsTransmogrified());
			}

			return false;
		}

        public IList<KeyValuePair<string, int>> GetConsumedItems(CompatibleIngredient compatibleIngredient, bool useMultipler, ref int resultMultiplier)
        {
			var ownedItems = compatibleIngredient.GetPrivateField<CompatibleIngredient, List<Item>>("m_ownedItems");
			var consumedItems = new List<KeyValuePair<string, int>>();

			if (ownedItems.Count == 1)
            {
				consumedItems.Add(new KeyValuePair<string, int>(ownedItems[0].UID, ownedItems[0].ItemID));
				return consumedItems;
            }
			var recipe = ParentCraftingMenu.GetSelectedRecipe();
			//Return first Non Transmogrified Item
			if (recipe is TransmogRecipe)
            {
				var item = ownedItems.First(i => !((UID)i.UID).IsTransmogrified());
				consumedItems.Add(new KeyValuePair<string, int>(item.UID, item.ItemID));
				return consumedItems;
			}

			//Return first Tranmog'd item
			if (recipe is TransmogRemoverRecipe)
			{
				var tmoggedItem = ownedItems.First(i => ((UID)i.UID).IsTransmogrified());
				consumedItems.Add(new KeyValuePair<string, int>(tmoggedItem.UID, tmoggedItem.ItemID));
				return consumedItems;
			}

			consumedItems.Add(new KeyValuePair<string, int>(ownedItems[0].UID, ownedItems[0].ItemID));
			return consumedItems;
		}
    }
}
