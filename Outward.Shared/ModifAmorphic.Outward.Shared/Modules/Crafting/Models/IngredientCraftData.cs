using System;
using System.Collections.Generic;
using System.Text;

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

        public Recipe MatchIngredientsRecipe { get; internal set; }

        public void Reset()
        {
            IngredientEnchantData.Clear();
            ConsumedItems.Clear();
            MatchIngredientsRecipe = null;
        }
    }
}
