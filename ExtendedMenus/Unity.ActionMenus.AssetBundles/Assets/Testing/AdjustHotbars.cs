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

    private HotbarsContainer _hotbarsContainer;
    private IHotbarController _hotbarController;
    private RandomActionAssignmentGenerator _actionGenerator;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AdjustHotbars: Getting HotbarsMain component from HotbarsMainGo. HotbarsMainGo is null? " + HotbarsGameObject == null);
        
        _hotbarsContainer = HotbarsGameObject.GetComponent<HotbarsContainer>();
        _hotbarController = _hotbarsContainer.Controller;
        Psp.GetServicesProvider(0).AddSingleton<IActionViewData>(ActionsViewUser);
        _hotbarController.ConfigureHotbars(1, 1, 8, SlotConfigService.GetActionSlotConfigs(1, 1, 8));
        _actionGenerator = new RandomActionAssignmentGenerator(_hotbarController);
        _actionGenerator.Generate(SlotIcons, this);

        Psp.GetServicesProvider(0).AddSingleton<IHotbarProfileDataService>(new TestHotbarProfileData());
    }
    

    public void IncrementHotbars()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount() + 1
            , _hotbarController.GetRowCount()
            , _hotbarController.GetActionSlotsPerRow()
            , SlotConfigService.GetActionSlotConfigs(_hotbarController.GetHotbarCount() + 1, _hotbarController.GetRowCount(), _hotbarController.GetActionSlotsPerRow()));
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementHotbars()
    {
        if (_hotbarController.GetHotbarCount() > 1)
        {
            _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount() - 1
                , _hotbarController.GetRowCount()
                , _hotbarController.GetActionSlotsPerRow()
                , SlotConfigService.GetActionSlotConfigs(_hotbarController.GetHotbarCount() - 1, _hotbarController.GetRowCount(), _hotbarController.GetActionSlotsPerRow()));
            _actionGenerator.Generate(SlotIcons, this);
        }
    }
    public void IncrementRows()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount()
            , _hotbarController.GetRowCount() + 1
            , _hotbarController.GetActionSlotsPerRow()
            , SlotConfigService.GetActionSlotConfigs(_hotbarController.GetHotbarCount(), _hotbarController.GetRowCount() + 1, _hotbarController.GetActionSlotsPerRow()));
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementRows()
    {
        if (_hotbarController.GetRowCount() > 1)
        {
            _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount()
                , _hotbarController.GetRowCount() - 1
                , _hotbarController.GetActionSlotsPerRow()
                , SlotConfigService.GetActionSlotConfigs(_hotbarController.GetHotbarCount(), _hotbarController.GetRowCount() - 1, _hotbarController.GetActionSlotsPerRow()));
            _actionGenerator.Generate(SlotIcons, this);
        }
    }
    public void IncrementActionSlots()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount()
            , _hotbarController.GetRowCount()
            , _hotbarController.GetActionSlotsPerRow() + 1
            , SlotConfigService.GetActionSlotConfigs(_hotbarController.GetHotbarCount(), _hotbarController.GetRowCount(), _hotbarController.GetActionSlotsPerRow() + 1));
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementActionSlots()
    {
        if (_hotbarController.GetActionSlotsPerRow() > 1)
        {
            _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount()
                , _hotbarController.GetRowCount()
                , _hotbarController.GetActionSlotsPerRow() - 1
                , SlotConfigService.GetActionSlotConfigs(_hotbarController.GetHotbarCount(), _hotbarController.GetRowCount(), _hotbarController.GetActionSlotsPerRow() - 1));
            _actionGenerator.Generate(SlotIcons, this);
        }
    }

    public void SelectNextHotbar() => _hotbarController.SelectNext();
    public void SelectPreviousHotbar() => _hotbarController.SelectPrevious();
    public void SelectHotbar(int hotbarIndex) => _hotbarController.SelectHotbar(hotbarIndex);

    public void TestStuff()
    {
        var ihd = new List<IHotbarData<ISlotData>>();
        var thd = new List<TestHotbarData>()
        {
            new TestHotbarData()
            {
                HotbarIndex = 0,
                SlotsAssigned = new List<ISlotData>()
                {
                    new TestSlotData()
                    {
                        SlotIndex = 0,
                    }
                }
            }
        };
        thd.Add(new TestHotbarData()
        {
            HotbarIndex = 1,
            SlotsAssigned = new List<ISlotData>()
                {
                    new TestSlotData()
                    {
                        SlotIndex = 0,
                    }
                }
        });

        var assigns = thd.Select(h => (IHotbarData<ISlotData>)h).ToList();

        var converted = new List<IHotbarData<ISlotData>>();
        foreach (var bar in thd)
        {
            converted.Add(bar);
        }
        ihd = converted;//assigns.ToList();
    }
}
public class TestHotbarData : IHotbarData<ISlotData>
{
    public int HotbarIndex { get; set; }
    public List<ISlotData> SlotsAssigned { get; set; }
}
public class TestSlotData : ISlotData
{
    public int SlotIndex { get; set; }
    public ActionSlotConfig Config { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

