using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients
{
    public interface IConsumedItemSelector
    {
        /// <summary>
        /// The crafting menu instance which owns the collection of ingredients this IConsumedItemSelector will process requests for.
        /// </summary>
        CustomCraftingMenu ParentCraftingMenu { get; set; }


        /// <summary>
        /// Invoked prior to destroying or "consuming" items when crafting a new item.  Results of this method could also be potentially used by a DynamicResultService if one is configured. A result is always expected. This
        /// method allows for a specific owned item to be chosen if there is more than one available.
        /// </summary>
        /// <param name="compatibleIngredient">The ingredient being examined as a potential ingredient for crefting.</param>
        /// <param name="useMultipler">Whether or not to use a multiplier. Almost always false unless water. Used by the Base game method to calculate how many should be consumed.</param>
        /// <param name="resultMultiplier">The calculated result multiplier. Usually 1 unless multiplier is true, in which case it will be total amount of owned items. Can be set to a different value, but normally left as.</param>
        /// <param name="consumedItems">A collection of (UID, ItemID) KeyValuePairs. This needs to return at least one valid item if a "true" result is returned.</param>
        /// <returns>True to use consumed items returned by this invocation. Otherwise false to use the base game calculation.</returns>
        bool TryGetConsumedItems(CompatibleIngredient compatibleIngredient, bool useMultipler, out int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems);
    }
}
