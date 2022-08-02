using System.Collections.Generic;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public interface IDynamicResultService
    {
        void SetDynamicItemID(DynamicCraftingResult craftingResult, int newItemId, ref int itemID, ref Item item);
        void CalculateResult(DynamicCraftingResult craftingResult, Recipe recipe, IEnumerable<CompatibleIngredient> ingredients);
    }
}
