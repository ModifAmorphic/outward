using ModifAmorphic.Outward.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CustomRecipe : Recipe
    {
        public string RecipeName => this.GetPrivateField<Recipe, string>("m_name");
        public string ResultName => Results.Length > 0 ? Results[0].RefItem?.DisplayName : string.Empty;

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
