using System;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Controllers
{
    public interface IActionSlotController
    {
        ActionSlot ActionSlot { get; }

        void AssignEmptyAction();
        void AssignSlotAction(ISlotAction slotAction);
        void Configure(IActionSlotConfig config);
        void Refresh();
        void ActionSlotUpdate();
        void ActionSlotAwake();
    }
}