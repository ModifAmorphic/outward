using ModifAmorphic.Outward.StashPacks.Settings;

namespace ModifAmorphic.Outward.StashPacks.Extensions
{
    public static class ItemExtensions
    {
        public static bool IsStashBag(this Item item)
        {
            return item is Bag && StashPacksConstants.StashBackpackItemIds.ContainsKey(item.ItemID);
        }
    }
}
