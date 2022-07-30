using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IActionViewData
    {
        IEnumerable<ISlotAction> GetAllActions();
        IEnumerable<IActionsDisplayTab> GetActionsTabData();
    }
}
