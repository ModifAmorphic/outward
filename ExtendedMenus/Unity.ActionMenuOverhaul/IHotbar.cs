using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IHotbar
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

        void ConfigureHotbar(int hotbars, int actionSlots);
        void ConfigureHotbar(int hotbars, int rows, int actionSlots);
    }
}