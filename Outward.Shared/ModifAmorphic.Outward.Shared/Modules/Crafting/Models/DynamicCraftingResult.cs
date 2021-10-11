using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Modules.Crafting.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class DynamicCraftingResult : ItemReferenceQuantity
    {
        IDynamicResultService _resultService;

        private int _dynamicItemID = -1;
        public int DynamicItemID { get => _dynamicItemID; private set => _dynamicItemID = value; }

        private Item _dynamicRefItem;
        public Item DynamicRefItem { get => _dynamicRefItem; private set => _dynamicRefItem = value; }

        private IngredientCraftData _ingredientCraftData;
        public IngredientCraftData IngredientCraftData => _ingredientCraftData;

        public DynamicCraftingResult(IDynamicResultService resultService, Item _item) : base(_item) => (_resultService) = (resultService);

        public DynamicCraftingResult(IDynamicResultService resultService, int _itemID) : base(_itemID) => (_resultService) = (resultService);

        public DynamicCraftingResult(IDynamicResultService resultService, Item _item, int _quantity) : base(_item, _quantity) => (_resultService) = (resultService);

        public DynamicCraftingResult(IDynamicResultService resultService, int _itemID, int _quantity) : base(_itemID, _quantity) => (_resultService) = (resultService);

        public void SetDynamicItemID(int itemID) => _resultService.SetDynamicItemID(this, itemID, ref _dynamicItemID, ref _dynamicRefItem);
        public void CalculateResult(Recipe recipe, IEnumerable<CompatibleIngredient> ingredients) => _resultService.CalculateResult(this, recipe, ingredients);
        public void SetIngredientData(IngredientCraftData craftData)
        {
            _ingredientCraftData = craftData;
        }
        public void ResetResult()
        {
            (_dynamicItemID, _dynamicRefItem) = (-1, null);
            _ingredientCraftData.Reset();
        }
    }
}
