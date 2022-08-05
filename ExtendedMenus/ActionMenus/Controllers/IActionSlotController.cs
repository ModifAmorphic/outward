using System;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Controllers
{
    public interface IActionSlotController
    {
        ActionSlot ActionSlot { get; }

        void AssignEmptyAction(Func<bool> getEditRequested = null);
        void AssignSlotAction(ISlotAction slotAction, Func<bool> getEditRequested = null);
        void Configure(IActionSlotConfig config);
        void Refresh();
        void ActionSlotUpdate();
        void ActionSlotAwake();
    }
}