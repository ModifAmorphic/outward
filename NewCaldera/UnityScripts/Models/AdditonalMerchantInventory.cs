using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts.Models
{

    public class AdditonalMerchantInventory : IMerchantInventory
    {
        public string Name { get; set; }
        public AreaEnum Area { get; set; }
        public NpcSearchTypes NpcSearchType { get; set; }
        public List<string> MerchantUIDs { get; set; }
        public List<string> ShopNames { get; set; }
        public List<string> ShopNpcNames { get; set; }
        public List<ItemAmounts> GuaranteedItems { get; set; }
    }

    public class MerchantInventoriesHolder
    {
        public List<AdditonalMerchantInventory> AdditonalMerchantInventories { get; set; } = new List<AdditonalMerchantInventory>();
    }
}
