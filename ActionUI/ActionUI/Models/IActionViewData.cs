using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IActionViewData
    {
        IEnumerable<ISlotAction> GetAllActions();
        IEnumerable<IActionsDisplayTab> GetActionsTabData();
    }
}
