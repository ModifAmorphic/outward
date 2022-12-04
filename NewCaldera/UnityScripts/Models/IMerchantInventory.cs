using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts.Models
{
    public enum NpcSearchTypes
    {
        ByUID,
        ByShopName,
        ByShopNpcName,
        All
    }

    public interface IMerchantInventory
    {
        AreaEnum Area { get; set; }
        NpcSearchTypes NpcSearchType { get; set; }
        List<string> MerchantUIDs { get; set; }
        List<string> ShopNames { get; set; }
        List<string> ShopNpcNames { get; set; }
        List<ItemAmounts> GuaranteedItems { get; set; }
    }
}
