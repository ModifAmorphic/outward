using ModifAmorphic.Outward.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CustomRecipe : Recipe
    {

        public void SetRecipeResults(IEnumerable<ItemReferenceQuantity> results)
        {
            this.SetPrivateField<Recipe, ItemReferenceQuantity[]>("m_results", results.ToArray());
        }
        public void SetRecipeResult(ItemReferenceQuantity result)
        {
            this.SetPrivateField<Recipe, ItemReferenceQuantity[]>("m_results", new ItemReferenceQuantity[1] { result });
        }
    }
}
