using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public class ActionSlotConfig
    {
        public string HotkeyText { get; set; }
        public bool ShowCooldownTime { get; set; } = true;
        public bool PreciseCooldownTime { get; set; } = true;
        public bool ShowZeroStackAmount { get; set; } = true;
        public EmptySlotOptions EmptySlotOption { get; set; } = EmptySlotOptions.Transparent;
    }
    public enum EmptySlotOptions
    {
        Transparent,
        Image,
        Hidden
    }
}
