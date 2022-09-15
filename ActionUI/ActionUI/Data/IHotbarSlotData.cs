using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IHotbarSlotData
    {
        int HotbarIndex { get; set; }
        string HotbarHotkey { get; set; }
        List<ISlotData> Slots { get; set; }
    }
}
