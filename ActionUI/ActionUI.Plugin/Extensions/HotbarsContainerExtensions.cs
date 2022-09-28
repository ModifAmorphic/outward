using ModifAmorphic.Outward.ActionUI.DataModels;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.Extensions
{
    internal static class HotbarsContainerExtensions
    {
        //public static ProfileData ToProfileData(this HotbarsContainer hbc, string name)
        //{
        //    var hotbarSlotData = hbc.ToHotbarSlotData();
        //    return new ProfileData()
        //    {
        //        Name = name,
        //        Rows = hbc.Controller.GetRowCount(),
        //        SlotsPerRow = hbc.Controller.GetActionSlotsPerRow(),
        //        Hotbars = hotbarSlotData,
        //    };
        //}
        public static List<IHotbarSlotData> ToHotbarSlotData(this HotbarsContainer hbc, HotbarData[] hotbarDatas)
        {
            var hotbars = new List<IHotbarSlotData>();

            for (int h = 0; h < hbc.Hotbars.Length; h++)
            {
                var hotbarData = new HotbarData();
                if (hotbarDatas.Length > h)
                {
                    hotbarData = hotbarDatas[h];
                    hotbarData.Slots.Clear();
                }
                else
                {
                    hotbarData.HotbarIndex = h;
                    hotbarData.RewiredActionId = RewiredConstants.ActionSlots.HotbarNavActions[h].id;
                    hotbarData.RewiredActionName = RewiredConstants.ActionSlots.HotbarNavActions[h].name;
                }
                for (int s = 0; s < hbc.Hotbars[h].Length; s++)
                {
                    hotbarData.Slots.Add(hbc.Hotbars[h][s].ToSlotData());
                }
                hotbars.Add(hotbarData);
            }
            return hotbars;
        }
    }
}
