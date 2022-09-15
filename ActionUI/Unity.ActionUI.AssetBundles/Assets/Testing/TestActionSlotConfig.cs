using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Testing
{
    internal class TestActionSlotConfig : IActionSlotConfig
    {
        public string HotkeyText { get; set; }
        public bool ShowCooldownTime { get; set; }
        public bool PreciseCooldownTime { get; set; }
        public bool ShowZeroStackAmount { get; set; }
        public EmptySlotOptions EmptySlotOption { get; set; }
    }
}
