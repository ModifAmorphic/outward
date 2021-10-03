using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public interface IDynamicResultService
    {
        //int GetQuantity(DynamicCraftingResult craftingResult);
        //int GetItemID(DynamicCraftingResult craftingResult);
        //Item GetRefItem(DynamicCraftingResult craftingResult);
        void SetDynamicItemID(DynamicCraftingResult craftingResult, int newItemId, ref int itemID, ref Item item);
        void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients);
    }
}
