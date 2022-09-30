using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public interface IActionsDisplayTab
    {
        string DisplayName { get; }
        int TabOrder { get; }
        IEnumerable<ISlotAction> GetSlotActions();

    }
}
