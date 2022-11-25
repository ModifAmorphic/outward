using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts
{
    [Serializable]
    public class ItemQuantityBinder : IBoundTypeConverter
    {
        public int ItemID = -1;
        public int Quantity = 1;

        public static  Type GetBindingType() => OutwardAssembly.Types.ItemQuantity;
        public Type GetBoundType() => GetBindingType();

        public object ToBoundType()
        {
            var type = GetBoundType();
            var itemQty = Activator.CreateInstance(type);

            itemQty.SetField(type, "Quantity", Quantity);
            if (ItemID != -1)
            {
                var item = ModifScriptsManager.Instance.PrefabManager.GetItemPrefab(ItemID);
                itemQty.SetField(type, "Item", item);
            }

            return itemQty;
        }
    }
}
