using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IActionsDisplayTab
    {
        string DisplayName { get; }
        int TabOrder { get; }
        IEnumerable<ISlotAction> GetSlotActions();

    }
}
