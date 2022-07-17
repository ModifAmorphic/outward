using ModifAmorphic.Outward.Unity.ActionMenus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustHotbars : MonoBehaviour
{
    public GameObject HotbarsGo;
    private Hotbars _hotbars;
    private Hotbars.HotbarType _hotbarType;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AdjustHotbars: Getting HotbarsMain component from HotbarsMainGo. HotbarsMainGo is null? " + HotbarsGo == null);
        _hotbars = HotbarsGo.GetComponent<Hotbars>();
        _hotbars.ConfigureHotbars(Hotbars.HotbarType.Single, 1, 8);
    }

    public void IncrementHotbars()
    {
        _hotbars.EnabledHotbar.ConfigureHotbar(_hotbars.EnabledHotbar.HotbarCount + 1, _hotbars.EnabledHotbar.ActionSlotsPerBar);
    }
    public void DecrementHotbars()
    {
        if (_hotbars.EnabledHotbar.HotbarCount > 1)
            _hotbars.EnabledHotbar.ConfigureHotbar(_hotbars.EnabledHotbar.HotbarCount - 1, _hotbars.EnabledHotbar.ActionSlotsPerBar);
    }
    public void IncrementRows()
    {
        _hotbars.EnabledHotbar.ConfigureHotbar(_hotbars.EnabledHotbar.HotbarCount, _hotbars.EnabledHotbar.RowCount + 1, _hotbars.EnabledHotbar.ActionSlotsPerBar);
    }
    public void DecrementRows()
    {
        if (_hotbars.EnabledHotbar.RowCount > 1)
            _hotbars.EnabledHotbar.ConfigureHotbar(_hotbars.EnabledHotbar.HotbarCount, _hotbars.EnabledHotbar.RowCount - 1, _hotbars.EnabledHotbar.ActionSlotsPerBar);
    }
    public void IncrementActionSlots()
    {
        _hotbars.EnabledHotbar.ConfigureHotbar(_hotbars.EnabledHotbar.HotbarCount, _hotbars.EnabledHotbar.ActionSlotsPerBar + 1);
    }
    public void DecrementActionSlots()
    {
        if (_hotbars.EnabledHotbar.ActionSlotsPerBar > 8)
        {
            _hotbars.EnabledHotbar.ConfigureHotbar(_hotbars.EnabledHotbar.HotbarCount, _hotbars.EnabledHotbar.ActionSlotsPerBar - 1);
        }
    }

    public void ChangeHotbarType(int option)
    {
        var hotbarType = option == 0 ? Hotbars.HotbarType.Grid : Hotbars.HotbarType.Single;

        _hotbars.ConfigureHotbars(hotbarType, _hotbars.EnabledHotbar.HotbarCount, _hotbars.EnabledHotbar.ActionSlotsPerBar);
    }
   

    public void SelectNextHotbar() => _hotbars.EnabledHotbar.SelectNext();
    public void SelectPreviousHotbar() => _hotbars.EnabledHotbar.SelectPrevious();
    public void SelectHotbar(int hotbarIndex) => _hotbars.EnabledHotbar.SelectHotbar(hotbarIndex);
}
