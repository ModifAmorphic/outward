using System;
using System.Collections.Generic;

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
        /// <param name="compatibleIngredient">The ingredient being examined as a potential ingredient for crafting.</param>
        /// <param name="useMultipler">Whether or not to use a multiplier. Almost always false unless water. Used by the Base game method to calculate how many should be consumed.</param>
        /// <param name="resultMultiplier">The calculated result multiplier. Usually 1 unless multiplier is true, in which case it will be total amount of owned items. Can be set to a different value, but normally left as.</param>
        /// <param name="consumedItems">A collection of (UID, ItemID) KeyValuePairs. This needs to return at least one valid item if a "true" result is returned.</param>
        /// <returns>True to use consumed items returned by this invocation. Otherwise false to use the base game calculation.</returns>
        bool TryGetConsumedItems(CompatibleIngredient compatibleIngredient, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems, out List<Item> preservedItems);

        /// <summary>
        /// Invoked prior to destroying or "consuming" items when crafting a new item if the item is correlated with a static ingredient. 
        /// Results of this method could also be potentially used by a DynamicResultService if one is configured. A result is always expected. This
        /// method allows for a specific owned item to be chosen if there is more than one available.
        /// </summary>
        /// <param name="compatibleIngredient">The ingredient being examined as a potential ingredient for crafting.</param>
        /// <param name="staticIngredientID">The ID of the ingredient that correlates it to the static ingredient the crafting menu was configured with.</param>
        /// <param name="useMultipler">Whether or not to use a multiplier. Almost always false unless water. Used by the Base game method to calculate how many should be consumed.</param>
        /// <param name="resultMultiplier">The calculated result multiplier. Usually 1 unless multiplier is true, in which case it will be total amount of owned items. Can be set to a different value, but normally left as.</param>
        /// <param name="consumedItems">A collection of (UID, ItemID) KeyValuePairs. This needs to return at least one valid item if a "true" result is returned.</param>
        /// <returns>True to use consumed items returned by this invocation. Otherwise false to use the base game calculation.</returns>
        bool TryGetConsumedStaticItems(CompatibleIngredient compatibleIngredient, Guid staticIngredientID, bool useMultipler, ref int resultMultiplier, out IList<KeyValuePair<string, int>> consumedItems, out List<Item> preservedItems);
    }
}
