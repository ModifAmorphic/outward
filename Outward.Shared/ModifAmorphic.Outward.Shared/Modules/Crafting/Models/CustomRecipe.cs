using ModifAmorphic.Outward.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CustomRecipe : Recipe
    {
        //public new ItemReferenceQuantity[] Results => GetResults(this.GetPrivateField<Recipe, ItemReferenceQuantity[]>("m_results"));

        public void SetRecipeResults(IEnumerable<ItemReferenceQuantity> results)
        {
            this.SetPrivateField<Recipe, ItemReferenceQuantity[]>("m_results", results.ToArray());
        }
        public void SetRecipeResult(ItemReferenceQuantity result)
        {
            this.SetPrivateField<Recipe, ItemReferenceQuantity[]>("m_results", new ItemReferenceQuantity[1] { result });
        }
        //get => this.GetPrivateField<ItemReferenceQuantity, ItemReferenceQuantity[]>("m_results");
        //set => this.SetPrivateField<ItemReferenceQuantity, ItemReferenceQuantity[]>("m_results", value);

        //protected virtual ItemReferenceQuantity[] GetResults(ItemReferenceQuantity[] baseResults) => baseResults;
    }
}
