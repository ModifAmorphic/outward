using Assets.Testing;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionMenus.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AdjustHotbars : MonoBehaviour
{
    public GameObject HotbarsGameObject;
    public Sprite[] SlotIcons;
    public ActionsViewUser ActionsViewUser;

    private IHotbarProfile _profile;
    private HotbarsContainer _hotbarsContainer;
    private IHotbarController _hotbarController;
    private RandomActionAssignmentGenerator _actionGenerator;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AdjustHotbars: Getting HotbarsMain component from HotbarsMainGo. HotbarsMainGo is null? " + HotbarsGameObject == null);
        _profile = TestDefaultProfile.DefaultProfile;
        _hotbarsContainer = HotbarsGameObject.GetComponent<HotbarsContainer>();
        _hotbarController = _hotbarsContainer.Controller;
        Psp.Instance.GetServicesProvider(0).AddSingleton<IActionViewData>(ActionsViewUser);
        _hotbarController.ConfigureHotbars(_profile);
        _actionGenerator = new RandomActionAssignmentGenerator(_hotbarController);
        _actionGenerator.Generate(SlotIcons, this);
        Psp.Instance.GetServicesProvider(0).AddSingleton<IHotbarProfileService>(new TestHotbarProfileDataService());
        Psp.Instance.GetServicesProvider(0).AddSingleton<IHotbarNavActions>(new HotbarNav());        
    }
    

    public void IncrementHotbars()
    {
        _profile.Hotbars.Add(new TestHotbarData()
        {
            HotbarIndex = _hotbarController.GetHotbarCount(),
            Slots = new List<ISlotData>() {},
        });
        _hotbarController.ConfigureHotbars(_profile);
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementHotbars()
    {
        if (_hotbarController.GetHotbarCount() > 1)
        {
            _profile.Hotbars.RemoveAt(_profile.Hotbars.Count - 1);
            _hotbarController.ConfigureHotbars(_profile);
        }
    }
    public void IncrementRows()
    {
        _hotbarController.ConfigureHotbars(_profile);
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementRows()
    {
        if (_hotbarController.GetRowCount() > 1)
        {
            _hotbarController.ConfigureHotbars(_profile);
            _actionGenerator.Generate(SlotIcons, this);
        }
    }
    public void IncrementActionSlots()
    {
        _hotbarController.ConfigureHotbars(_profile);
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementActionSlots()
    {
        if (_hotbarController.GetActionSlotsPerRow() > 1)
        {
            _hotbarController.ConfigureHotbars(_profile);
            _actionGenerator.Generate(SlotIcons, this);
        }
    }

    public void SelectNextHotbar() => _hotbarController.SelectNext();
    public void SelectPreviousHotbar() => _hotbarController.SelectPrevious();
    public void SelectHotbar(int hotbarIndex) => _hotbarController.SelectHotbar(hotbarIndex);

}
public class TestHotbarData : IHotbarSlotData
{
    public int HotbarIndex { get; set; }
    public List<ISlotData> Slots { get; set; }
    public string HotbarHotkey { get; set; }
}
public class TestSlotData : ISlotData
{
    public int SlotIndex { get; set; }
    public IActionSlotConfig Config { get; set; }
}

