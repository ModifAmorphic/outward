using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class AvailableIngredientFilter
    {
		public enum EnchantFilters
        {
			/// <summary>
			/// Exclude Enchanted items when filtering available ingredient.
			/// </summary>
			ExcludeEnchanted,
			/// <summary>
			/// Include Enchanted items when filtiring for available ingredients.
			/// </summary>
			IncludeEnchanted,
			/// <summary>
			/// Only include enchanted items when filtering for available ingredients.
			/// </summary>
			OnlyEnchanted
		}
		/// <summary>
		/// Used to filter items from inventory for all recipes. Base game code uses this tag 
		/// and the crafting ingredient tag. Can be set to any registered tag. 
		/// Default of tag is not set unless otherwise specified.
		/// </summary>
		public Tag InventoryFilterTag { get; set; }

		/// <summary>
		/// Whether or not enchanted items should be included.
		/// </summary>
		public EnchantFilters EnchantFilter { get; set; }

		/// <summary>
		/// Allows for matching based on an Item's exact type and inheritance chain.
		/// For example, adding typeof(<see cref="Equipment"/>) would also allow types typeof(<see cref="Weapon"/>) and
		/// typeof(<see cref="Armor"/>) through, since those are both derived subclasses of <see cref="Equipment"/>.
		/// If no type is added, will default to typeof(<see cref="Item"/>) on first use.
		/// </summary>
		public HashSet<Type> ItemTypes { get; set; } = new HashSet<Type>();
	}
}
