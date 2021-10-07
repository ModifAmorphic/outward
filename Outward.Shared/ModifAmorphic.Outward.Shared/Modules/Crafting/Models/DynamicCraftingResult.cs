using ModifAmorphic.Outward.Extensions;
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

        private readonly Dictionary<int, string> _ingredientEnchantData = new Dictionary<int, string>();
        public IReadOnlyDictionary<int, string> IngredientEnchantData { get => _ingredientEnchantData; }

        public DynamicCraftingResult(IDynamicResultService resultService, Item _item) : base(_item) => (_resultService) = (resultService);

        public DynamicCraftingResult(IDynamicResultService resultService, int _itemID) : base(_itemID) => (_resultService) = (resultService);

        public DynamicCraftingResult(IDynamicResultService resultService, Item _item, int _quantity) : base(_item, _quantity) => (_resultService) = (resultService);

        public DynamicCraftingResult(IDynamicResultService resultService, int _itemID, int _quantity) : base(_itemID, _quantity) => (_resultService) = (resultService);

        public void SetDynamicItemID(int itemID) => _resultService.SetDynamicItemID(this, itemID, ref _dynamicItemID, ref _dynamicRefItem);
        public void CalculateResult(Recipe recipe, IEnumerable<CompatibleIngredient> ingredients) => _resultService.CalculateResult(this, recipe, ingredients);
        public void SetEnchantData(Dictionary<int, string> ingredientEnchantData)
        {
            _ingredientEnchantData.Clear();
            _ingredientEnchantData.Merge(ingredientEnchantData);
        }
        public void ResetResult()
        {
            (_dynamicItemID, _dynamicRefItem) = (-1, null);
            _ingredientEnchantData.Clear();
        }
    }
}
