using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Controllers
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
        //void ConfigureHotbars(int hotbars, int rows, int slotsPerRow, IActionSlotConfig[,] slotConfigs);

        void ToggleActionSlotEdits(bool enabled);
        void ToggleHotkeyEdits(bool enabled);

    }
}