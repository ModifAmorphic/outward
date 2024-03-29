﻿using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface IHotbarProfile
    {
        List<IHotbarSlotData> Hotbars { get; set; }
        int Rows { get; set; }
        int SlotsPerRow { get; set; }
        bool HideLeftNav { get; set; }
        bool CombatMode { get; set; }
        string NextHotkey { get; set; }
        string PrevHotkey { get; set; }
        int Scale { get; set; }
    }
}