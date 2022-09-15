using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IHotbarProfile
    {
        List<IHotbarSlotData> Hotbars { get; set; }
        int Rows { get; set; }
        int SlotsPerRow { get; set; }
        bool CombatMode { get; set; }
        string NextHotkey { get; set; }
        string PrevHotkey { get; set; }
    }
}