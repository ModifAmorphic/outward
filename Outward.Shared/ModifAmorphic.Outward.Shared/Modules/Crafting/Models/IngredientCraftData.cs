using System.Collections.Generic;

namespace ModifAmorphic.Outward.Modules.Crafting.Models
{
    public class IngredientCraftData
    {
        public Dictionary<int, string> IngredientEnchantData { get; protected set; } = new Dictionary<int, string>();
        /// <summary>
        /// Consumed Items are calculated immediately before being destroyed / consumed, but before the result is crafted.<br />
        /// ItemID<br />
        /// --Item's UID<br />
        /// --Quantity<br />
        /// </summary>
        public Dictionary<int, Dictionary<string, int>> ConsumedItems { get; protected set; } = new Dictionary<int, Dictionary<string, int>>();
        /// <summary>
        /// Preserved Items are in game, realized <see cref="Item"/>s that were not consumed / destroyed prior to crafting. This List will be populated
        /// with any unconsumed items after the craft button has been pressed until after the item has been crafted.
        /// </summary>
        public List<Item> PreservedItems { get; protected set; } = new List<Item>();

        public Recipe MatchIngredientsRecipe { get; internal set; }

        public void Reset()
        {
            IngredientEnchantData.Clear();
            ConsumedItems.Clear();
            PreservedItems.Clear();
            MatchIngredientsRecipe = null;
        }
    }
}
