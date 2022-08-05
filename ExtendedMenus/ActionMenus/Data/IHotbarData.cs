using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    //public interface IHotbarSlotData<T> where T : ISlotData
    //{
    //    int HotbarIndex { get; set; }
    //    List<T> SlotsAssigned { get; set; }
    //}
    public interface IHotbarSlotData
    {
        int HotbarIndex { get; set; }
        List<ISlotData> Slots { get; set; }
    }
}
