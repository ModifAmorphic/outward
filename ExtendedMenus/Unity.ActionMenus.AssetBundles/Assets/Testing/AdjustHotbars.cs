using Assets.Testing;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using System.Collections;
using System.Collections.Generic;
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
        _hotbarController.RegisterActionViewData(ActionsViewUser);
        _hotbarController.ConfigureHotbars(1, 1, 8);
        _actionGenerator = new RandomActionAssignmentGenerator(_hotbarController);
        _actionGenerator.Generate(SlotIcons, this);

    }

    public void IncrementHotbars()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount() + 1, _hotbarController.GetRowCount(), _hotbarController.GetActionSlotsPerBar());
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementHotbars()
    {
        if (_hotbarController.GetHotbarCount() > 1)
        {
            _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount() - 1, _hotbarController.GetRowCount(), _hotbarController.GetActionSlotsPerBar());
            _actionGenerator.Generate(SlotIcons, this);
        }
    }
    public void IncrementRows()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount(), _hotbarController.GetRowCount() + 1, _hotbarController.GetActionSlotsPerBar());
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementRows()
    {
        if (_hotbarController.GetRowCount() > 1)
        {
            _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount(), _hotbarController.GetRowCount() - 1, _hotbarController.GetActionSlotsPerBar());
            _actionGenerator.Generate(SlotIcons, this);
        }
    }
    public void IncrementActionSlots()
    {
        _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount(), _hotbarController.GetRowCount(), _hotbarController.GetActionSlotsPerBar() + 1);
        _actionGenerator.Generate(SlotIcons, this);
    }
    public void DecrementActionSlots()
    {
        if (_hotbarController.GetActionSlotsPerBar() > 1)
        {
            _hotbarController.ConfigureHotbars(_hotbarController.GetHotbarCount(), _hotbarController.GetRowCount(), _hotbarController.GetActionSlotsPerBar() - 1);
            _actionGenerator.Generate(SlotIcons, this);
        }
    }

    public void SelectNextHotbar() => _hotbarController.SelectNext();
    public void SelectPreviousHotbar() => _hotbarController.SelectPrevious();
    public void SelectHotbar(int hotbarIndex) => _hotbarController.SelectHotbar(hotbarIndex);
}
