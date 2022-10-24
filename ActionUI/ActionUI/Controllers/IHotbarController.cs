using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionUI.Controllers
{
    public interface IHotbarController
    {
        GridLayoutGroup[] GetHotbarGrids();
        ActionSlot[][] GetActionSlots();
        int GetRowCount();
        int GetHotbarCount();
        int GetActionSlotsPerRow();
        int GetActionSlotsPerBar();
        void HotbarsContainerUpdate();

        void SelectHotbar(int hotbarIndex);
        void SelectNext();
        void SelectPrevious();

        void ConfigureHotbars(IHotbarProfile profile);

        void EnableHotbars();
        void DisableHotbars();
        void ReassignActionSlots();
        //void ConfigureHotbars(int hotbars, int rows, int slotsPerRow, IActionSlotConfig[,] slotConfigs);

        void ToggleActionSlotEdits(bool enabled);
        void ToggleHotkeyEdits(bool enabled);

    }
}