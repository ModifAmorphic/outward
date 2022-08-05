using ModifAmorphic.Outward.Unity.ActionMenus.Data;
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
        int GetActionSlotsPerRow();
        int GetActionSlotsPerBar();
        void HotbarsContainerUpdate();

        void SelectHotbar(int hotbarIndex);
        void SelectNext();
        void SelectPrevious();

        void ConfigureHotbars(IHotbarProfileData profile, IHotbarRequestActions requestActions);
        void ConfigureHotbars(int hotbars, int rows, int slotsPerRow, IActionSlotConfig[,] slotConfigs);
        void ConfigureHotbars(int hotbars, int rows, int slotsPerRow, IActionSlotConfig[,] slotConfigs, IHotbarRequestActions requestActions);

        void ToggleEditMode(bool enabled);
    }
}