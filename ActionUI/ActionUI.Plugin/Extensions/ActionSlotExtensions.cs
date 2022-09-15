using ModifAmorphic.Outward.UI.DataModels;
using ModifAmorphic.Outward.UI.Models;
using ModifAmorphic.Outward.Unity.ActionMenus;

namespace ModifAmorphic.Outward.UI.Extensions
{
    internal static class ActionSlotExtensions
    {
        public static SlotData ToSlotData(this ActionSlot actionSlot)
        {
            var sData = new SlotData()
            {
                SlotIndex = actionSlot.SlotIndex,
                Config = actionSlot.Config,
            };

            if (actionSlot.SlotAction is IOutwardItem islot)
            {
                sData.ItemID = islot.ActionItem.ItemID;
                sData.ItemUID = islot.ActionItem.UID;
            }
            return sData;
        }
    }
}
