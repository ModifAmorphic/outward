using ModifAmorphic.Outward.Unity.ActionMenus;

namespace ModifAmorphic.Outward.Unity.ActionUI.Controllers
{
    public interface IActionSlotController
    {
        ActionSlot ActionSlot { get; }

        void AssignEmptyAction();
        void AssignSlotAction(ISlotAction slotAction, bool surpressChangeFlag = false);
        void Configure(IActionSlotConfig config);
        void Refresh();
        void ActionSlotUpdate();
        void ActionSlotAwake();
        void ActionSlotOnDisable();
        void ToggleEditMode(bool toggle);
        void ToggleHotkeyEditMode(bool toggle);
        void HideCooldown();
        void HideSlider(BarPositions slider);
    }
}