using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Testing
{
    internal class ActionsDisplayTab : IActionsDisplayTab
    {
        public string DisplayName { get; set; }
        public int TabOrder { get; set; }

        private IEnumerable<ISlotAction> _slotActions;

        public ActionsDisplayTab(ActionsViewUser viewData, string displayName, int order)
        {
            DisplayName = displayName;
            TabOrder = order;
            _slotActions = viewData.GetAllActions();
        }

        public IEnumerable<ISlotAction> GetSlotActions() => _slotActions;
    }
}
