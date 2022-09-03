using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IActionMenusProfile
    {
        string Name { get; set; }
        bool ActionSlotsEnabled { get; set;  }
        bool DurabilityDisplayEnabled { get; set;  }
    }
}
