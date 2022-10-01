using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public interface IActionViewData
    {
        IEnumerable<ISlotAction> GetAllActions();
        IEnumerable<IActionsDisplayTab> GetActionsTabData();
    }
}
