using ModifAmorphic.Outward.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class DynamicCraftingResult : ItemReferenceQuantity
    {
        IDynamicResultService _resultService;

        //public new int ItemID => DynamicItemID;
        //public new int Quantity => DynamicQuantity;
        //public new Item RefItem => DynamicRefItem;

        private int _dynamicItemID = -1;
        public int DynamicItemID { get => _dynamicItemID; private set => _dynamicItemID = value; }
        //public int DynamicQuantity { get; set; }

        private Item _dynamicRefItem;
        public Item DynamicRefItem { get => _dynamicRefItem; private set => _dynamicRefItem = value; }
        //public Item DynamicRefItem { get; }

        private readonly int _recipeItemID;

        public DynamicCraftingResult(IDynamicResultService resultService, Item _item) : base(_item) => (_resultService, _recipeItemID) = (resultService, _item.ItemID);

        public DynamicCraftingResult(IDynamicResultService resultService, int _itemID) : base(_itemID) => (_resultService, _recipeItemID) = (resultService, _itemID);

        public DynamicCraftingResult(IDynamicResultService resultService, Item _item, int _quantity) : base(_item, _quantity) => (_resultService, _recipeItemID) = (resultService, _item.ItemID);

        public DynamicCraftingResult(IDynamicResultService resultService, int _itemID, int _quantity) : base(_itemID, _quantity) => (_resultService, _recipeItemID) = (resultService, _itemID);

        //private void Awake() => _resultService = CustomCraftingModule.GetDynamicResultService(this);

        public void SetDynamicItemID(int itemID) => _resultService.SetDynamicItemID(this, itemID, ref _dynamicItemID, ref _dynamicRefItem);
        public void CalculateResult(Recipe recipe, IEnumerable<CompatibleIngredient> ingredients) => _resultService.CalculateResult(this, recipe, ingredients);
        public void ResetResult() => (_dynamicItemID, _dynamicRefItem) = (-1, null);
    }
}
