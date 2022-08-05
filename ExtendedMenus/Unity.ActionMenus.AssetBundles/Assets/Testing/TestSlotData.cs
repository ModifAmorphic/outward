using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Testing
{
    internal class TestSlotData : ISlotData
    {
        public int SlotIndex { get; set; }
        public ActionSlotConfig Config { get; set; }
    }
}
