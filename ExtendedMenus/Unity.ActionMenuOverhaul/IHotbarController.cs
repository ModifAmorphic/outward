using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IHotbarController
    {
        GameObject GameObject { get; }
        GameObject[][] HotbarSlots { get; }
        int ActionSlotsPerBar { get; }
        int HotbarCount { get; }
        int RowCount { get; }
        int SelectedHotbar { get; }

        void SelectHotbar(int hotbarIndex);
        void SelectNext();
        void SelectPrevious();

        event Action<float> OnResizeWidthRequest;
        event Action<int> OnHotbarSelected;

        void ConfigureHotbars(int hotbars, int actionSlots);
        void ConfigureHotbars(int hotbars, int rows, int actionSlots);
    }
}