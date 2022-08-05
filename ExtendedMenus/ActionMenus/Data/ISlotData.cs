using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface ISlotData
    {
        int SlotIndex { get; set; }
        IActionSlotConfig Config { get; set; }
        //int ItemId { get; set; }
        //public UID ItemUID { get; set; }
    }
}
