using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IHotbarSlotData
    {
        int HotbarIndex { get; set; }
        List<ISlotData> Slots { get; set; }
    }
}
