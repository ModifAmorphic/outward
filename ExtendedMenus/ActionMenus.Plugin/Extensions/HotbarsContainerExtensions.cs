using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Extensions
{
    internal static class HotbarsContainerExtensions
    {
        //public static HotbarProfileData ToHotbarProfileData(this HotbarsContainer hbc, string name)
        //{
        //    var hotbarSlotData = hbc.ToHotbarSlotData();
        //    return new HotbarProfileData()
        //    {
        //        Name = name,
        //        Rows = hbc.Controller.GetRowCount(),
        //        SlotsPerRow = hbc.Controller.GetActionSlotsPerBar(),
        //        Hotbars = hotbarSlotData
        //    };
        //}
        public static ProfileData ToProfileData(this HotbarsContainer hbc, string name)
        {
            var hotbarSlotData = hbc.ToHotbarSlotData();
            return new ProfileData()
            {
                Name = name,
                Rows = hbc.Controller.GetRowCount(),
                SlotsPerRow = hbc.Controller.GetActionSlotsPerBar(),
                Hotbars = hotbarSlotData
            };
        }
        //public static List<HotbarData> ToHotbarData(this HotbarsContainer hbc)
        //{
        //    var hotbars = new List<HotbarData>();

        //    for (int h = 0; h < hbc.Hotbars.Length; h++)
        //    {
        //        var hotbarData = new HotbarData()
        //        {
        //            HotbarIndex = h
        //        };
        //        for (int s = 0; s < hbc.Hotbars[h].Length; s++)
        //        {
        //            hotbarData.SlotsAssigned.Add(hbc.Hotbars[h][s].ToSlotData());
        //        }
        //        hotbars.Add(hotbarData);
        //    }

        //    return hotbars;
        //}
        public static List<IHotbarSlotData> ToHotbarSlotData(this HotbarsContainer hbc)
        {
            var hotbars = new List<IHotbarSlotData>();

            for (int h = 0; h < hbc.Hotbars.Length; h++)
            {
                var hotbarData = new HotbarSlotData()
                {
                    HotbarIndex = h
                };
                for (int s = 0; s < hbc.Hotbars[h].Length; s++)
                {
                    hotbarData.Slots.Add(hbc.Hotbars[h][s].ToSlotData());
                }
                hotbars.Add(hotbarData);
            }
            return hotbars;
        }
        //public static List<IHotbarSlotData<ISlotData>> ToIHotbarSlotData(this HotbarsContainer hbc)
        //{
        //    var hotbars = new List<IHotbarSlotData<ISlotData>>();

        //    for (int h = 0; h < hbc.Hotbars.Length; h++)
        //    {
        //        var hotbarData = new HotbarData()
        //        {
        //            HotbarIndex = h
        //        };
        //        for (int s = 0; s < hbc.Hotbars[h].Length; s++)
        //        {
        //            hotbarData.SlotsAssigned.Add(hbc.Hotbars[h][s].ToSlotData());
        //        }
        //        hotbars.Add(hotbarData);
        //    }
        //    return hotbars;
        //}
    }
}
