using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.Models
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
