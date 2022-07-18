using ModifAmorphic.Outward.Unity.ActionMenus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustHotbars : MonoBehaviour
{
    public GameObject HotbarsGo;
    private HotbarsContainer _hotbarsContainer;
    private IHotbarController _hotbarController;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AdjustHotbars: Getting HotbarsMain component from HotbarsMainGo. HotbarsMainGo is null? " + HotbarsGo == null);
        _hotbarsContainer = HotbarsGo.GetComponent<HotbarsContainer>();
        _hotbarController = _hotbarsContainer.Hotbar;
        _hotbarController.ConfigureHotbars(1, 1, 8);
    }

    public void IncrementHotbars()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.HotbarCount + 1, _hotbarController.RowCount, _hotbarController.ActionSlotsPerBar);
    }
    public void DecrementHotbars()
    {
        if (_hotbarController.HotbarCount > 1)
            _hotbarController.ConfigureHotbars(_hotbarController.HotbarCount - 1, _hotbarController.RowCount, _hotbarController.ActionSlotsPerBar);
    }
    public void IncrementRows()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.HotbarCount, _hotbarController.RowCount + 1, _hotbarController.ActionSlotsPerBar);
    }
    public void DecrementRows()
    {
        if (_hotbarController.RowCount > 1)
            _hotbarController.ConfigureHotbars(_hotbarController.HotbarCount, _hotbarController.RowCount - 1, _hotbarController.ActionSlotsPerBar);
    }
    public void IncrementActionSlots()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.HotbarCount, _hotbarController.RowCount, _hotbarController.ActionSlotsPerBar + 1);
    }
    public void DecrementActionSlots()
    {
        if (_hotbarController.ActionSlotsPerBar > 8)
        {
            _hotbarController.ConfigureHotbars(_hotbarController.HotbarCount, _hotbarController.RowCount, _hotbarController.ActionSlotsPerBar - 1);
        }
    }

    public void SelectNextHotbar() => _hotbarController.SelectNext();
    public void SelectPreviousHotbar() => _hotbarController.SelectPrevious();
    public void SelectHotbar(int hotbarIndex) => _hotbarController.SelectHotbar(hotbarIndex);
}
