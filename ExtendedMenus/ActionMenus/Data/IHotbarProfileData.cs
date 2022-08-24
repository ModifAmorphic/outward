using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IHotbarProfileData
    {
        List<IHotbarSlotData> Hotbars { get; set; }
        string Name { get; set; }
        int Rows { get; set; }
        int SlotsPerRow { get; set; }
        bool CombatMode { get; set; }
        string NextHotkey { get; set; }
        string PrevHotkey { get; set; }
    }
}