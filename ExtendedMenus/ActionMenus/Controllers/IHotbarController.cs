using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Controllers
{
    public interface IHotbarController
    {
        GridLayoutGroup[] GetHotbarGrids();
        ActionSlot[][] GetActionSlots();
        int GetRowCount();
        int GetHotbarCount();
        int GetActionSlotsPerBar();
        void HotbarsContainerUpdate();

        void SelectHotbar(int hotbarIndex);
        void SelectNext();
        void SelectPrevious();

        void ConfigureHotbars(int hotbars, int rows, int slotsPerRow, IHotbarRequestActions requestActions = null);

        void ToggleEditMode(bool enabled);
        void RegisterActionViewData(IActionViewData actionViewData);
    }
}