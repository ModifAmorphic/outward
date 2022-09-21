using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Testing
{
    internal class TestHotbarProfileData : IHotbarProfile
    {
        public List<IHotbarSlotData> Hotbars { get; set; }
        public string Name { get; set; } = "Profile 1";
        public int Rows { get; set; } = 1;
        public int SlotsPerRow { get; set; } = 8;
        public string NextHotkey { get; set; } = ".";
        public string PrevHotkey { get; set; } = ",";
        public bool CombatMode { get; set; } = true;
    }
}
