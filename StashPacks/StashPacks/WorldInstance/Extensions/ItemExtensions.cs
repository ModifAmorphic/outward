using ModifAmorphic.Outward.StashPacks.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions
{
    public static class ItemExtensions
    {
        public static bool IsStashBag(this Item item)
        {
            return item is Bag && StashPacksConstants.StashBackpackItemIds.ContainsKey(item.ItemID);
        }
    }
}
