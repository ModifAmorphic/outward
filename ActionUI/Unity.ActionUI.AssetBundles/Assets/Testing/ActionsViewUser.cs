using Assets.Testing;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActionsViewUser : MonoBehaviour, IActionViewData
{
    public ActionsViewer ActionsViewer;
    //public GameObject InventoryGrid;
    //public GameObject HotbarsGo;
    public Sprite[] SlotIcons;

    //private GameObject _baseGridAction;

    private string[] _slotNames = new string[17]
    {
        "Cleanse",
        "Flamethrower",
        "Haunting Beat",
        "Jinx",
        "Mana Push",
        "Mana Ward",
        "Nurturing Echo",
        "Prime",
        "Kick",
        "Push Kick",
        "Reveal Soul",
        "Reverberation",
        "Spark",
        "Sweep Kick",
        "Throw Lantern",
        "Unerring Read",
        "Welkin Ring"
    };

    private RandomSlotActionGenerator _actionGenerator;

    private List<IActionsDisplayTab> _displayTabs = new List<IActionsDisplayTab>();
    private void Awake()
    {
        _actionGenerator = new RandomSlotActionGenerator(SlotIcons, _slotNames, this);
        int order = 0;
        _displayTabs.Add(new ActionsDisplayTab(this, "Skills", order++));
        _displayTabs.Add(new ActionsDisplayTab(this, "Consumables", order++));
        _displayTabs.Add(new ActionsDisplayTab(this, "Deployables", order++));
        _displayTabs.Add(new ActionsDisplayTab(this, "Equipment", order++));
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReloadGrid()
    {
        var actions = _actionGenerator.Generate(20);

        for(int i = 0; i < actions.Count; i++)
        {
            ActionsViewer.AddActionView(actions[i]);
        }
    }

    public IEnumerable<ISlotAction> GetAllActions()
    {
        var actions = _actionGenerator.Generate(125);
        return actions;
    }

    public IEnumerable<IActionsDisplayTab> GetActionsTabData() => _displayTabs;
}
