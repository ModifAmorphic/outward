using System;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Controllers
{
    public interface IActionSlotController
    {
        ActionSlot ActionSlot { get; }
        ISlotAction SlotAction { get; }

        void AssignEmptyAction(Func<bool> getEditRequested = null);
        void AssignSlotAction(ISlotAction slotAction, Func<bool> getEditRequested = null);
        void Configure(ActionSlotConfig config);
        void Refresh();
        void OnActionSlotUpdate();
    }
}