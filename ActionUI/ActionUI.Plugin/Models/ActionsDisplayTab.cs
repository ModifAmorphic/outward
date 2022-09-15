using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UI.Models
{
    internal class ActionsDisplayTab : IActionsDisplayTab
    {
        public string DisplayName { get; internal set; }
        public int TabOrder { get; internal set; }

        public Func<IEnumerable<ISlotAction>> GetSlotActionsQuery { get; internal set; }
        public IEnumerable<ISlotAction> GetSlotActions()
        {
            return GetSlotActionsQuery.Invoke();
        }
    }
}
