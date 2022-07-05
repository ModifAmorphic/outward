using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
	/// <summary>
	/// Menu Ingredient Filters are used to select ingredients that are available for use by a
	/// <see cref="CustomCraftingMenu"/>. These filters should, when combined, return all ingredients 
	/// required for any <see cref="Recipe"/> that is registered to this menu.
	/// </summary>
    public class MenuIngredientFilters
    {
		/// <summary>
		/// Used by the base game's code to filter items from inventory for all recipes. Base game code uses this tag 
		/// and the crafting ingredient tag. Defaults to
		/// the crafting station, but can be set to any registered tag.
		/// </summary>
		public Tag BaseInventoryFilterTag { get; set; }

		/// <summary>
		/// If set, character inventory will be searched again using the 
		/// provided filter. Any additional items will be added to the available Ingredients
		/// gathered by the base game code.
		/// </summary>
		public AvailableIngredientFilter AdditionalInventoryIngredientFilter { get; set; }

		/// <summary>
		/// If set, an additional ingredient search will be performed on equipped items.
		/// </summary>
		public AvailableIngredientFilter EquippedIngredientFilter { get; set; }
	}
}
