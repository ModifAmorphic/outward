using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IActionsDisplayTab
    {
        string DisplayName { get; }
        int TabOrder { get; }
        IEnumerable<ISlotAction> GetSlotActions();

    }
}
