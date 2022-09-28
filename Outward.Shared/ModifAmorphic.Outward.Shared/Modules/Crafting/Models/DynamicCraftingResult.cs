using ModifAmorphic.Outward.Modules.Crafting.Models;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class DynamicCraftingResult : ItemReferenceQuantity
    {
        private readonly IDynamicResultService _resultService;
        public IDynamicResultService ResultService => _resultService;

        private int _dynamicItemID = -1;
        public int DynamicItemID { get => _dynamicItemID; private set => _dynamicItemID = value; }

        private Item _dynamicRefItem;
        public Item DynamicRefItem { get => _dynamicRefItem; private set => _dynamicRefItem = value; }

        public Item ResultItem { get; private set; }
        public int ResultQuantity { get; private set; }

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
            (_dynamicItemID, _dynamicRefItem, ResultItem, ResultQuantity) = (-1, null, null, 0);
            _ingredientCraftData.Reset();
        }
        public void SetResultItems(Item result, int quantity)
        {
            ResultItem = result;
            ResultQuantity = quantity;
        }
    }
}
