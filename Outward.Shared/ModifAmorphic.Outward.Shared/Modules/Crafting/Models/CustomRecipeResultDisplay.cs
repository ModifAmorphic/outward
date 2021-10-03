using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CustomRecipeResultDisplay : RecipeResultDisplay
    {
        private Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory?.Invoke() ?? new NullLogger();

        private ItemReferenceQuantity _result
        {
            get => this.GetPrivateField<RecipeResultDisplay, ItemReferenceQuantity>("m_result");
            set => this.SetPrivateField<RecipeResultDisplay, ItemReferenceQuantity>("m_result", value);
        }
        
        //public override void SetReferencedItem(Item _item)
        //{
        //    SetCustomRecipeResult(_result);
        //    base.SetReferencedItem(_result.RefItem);
        //}

        public void SetCustomRecipeResult(ItemReferenceQuantity _result)
        {
            Logger.LogTrace($"{nameof(CustomRecipeResultDisplay)}::{nameof(SetCustomRecipeResult)}: Invoked for Result ItemID {_result?.ItemID}. " +
                $"_result is DynamicCraftingResult dynamicResult ? {_result is DynamicCraftingResult}.  " +
                $"this.ParentMenu is CustomCraftingMenu craftingMenu? {this.ParentMenu is CustomCraftingMenu}");

            if (_result == null ||
                !(_result is DynamicCraftingResult dynamicResult) || 
                !(this.ParentMenu is CustomCraftingMenu craftingMenu) || 
                craftingMenu.GetSelectedRecipe() == null)
            {
                return;
            }
            var ingredients = craftingMenu.GetSelectedIngredients();
            if (ingredients == null || !ingredients.Any())
                return;

            dynamicResult.CalculateResult(craftingMenu.GetSelectedRecipe(), ingredients);

            base.SetReferencedItem(dynamicResult.RefItem);;
        }
        public void SetLoggerFactory(Func<IModifLogger> loggerFactory) => _loggerFactory = loggerFactory;
    }
}
