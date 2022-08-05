using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;

namespace ModifAmorphic.Outward.ActionMenus.Extensions
{
    internal static class ProfileDataExtensions
    {
        //public static HotbarProfileData ToHotbarProfileData(this ProfileData profileData)
        //{
        //    var hpd = new HotbarProfileData()
        //    {
        //        Name = profileData.Name,
        //        Rows = profileData.Rows,
        //        SlotsPerRow = profileData.SlotsPerRow,
        //    };
        //    foreach(var p in profileData.Hotbars)
        //    {
        //        var slots = p.SlotsAssigned.Select(s => (ISlotData)s);
        //        var hotbarData = new HotbarData()
        //        {
        //             SlotsAssigned = slots.ToList(),
        //             HotbarIndex = p.HotbarIndex
        //        };
                
        //        hpd.Hotbars.Add(hotbarData);
        //    }
        //    return hpd;
        //}
    }
}
