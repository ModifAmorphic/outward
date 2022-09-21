using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface IHotbarSlotData
    {
        int HotbarIndex { get; set; }
        string HotbarHotkey { get; set; }
        List<ISlotData> Slots { get; set; }
    }
}
