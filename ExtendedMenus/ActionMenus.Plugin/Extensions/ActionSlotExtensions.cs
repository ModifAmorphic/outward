using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Extensions
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

            if (actionSlot.SlotAction is ItemSlotAction islot)
            {
                sData.ItemID = islot.ActionItem.ItemID;
                sData.ItemUID = islot.ActionItem.UID;
            }
            return sData;
        }
    }
}
