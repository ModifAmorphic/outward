using ModifAmorphic.Outward.StashPacks.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions
{
    public static class BagExtensions
    {
        public static void EmptyContents(this Bag stashBag)
        {
            stashBag.Container.ClearPouch();
            stashBag.Container.RemoveAllSilver();
        }
        public static bool IsStashBag(this Bag bag)
        {
            return StashPacksConstants.StashBackpackItemIds.ContainsKey(bag.ItemID);
        }
    }
}
