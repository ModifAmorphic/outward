using ModifAmorphic.Outward.Unity.ActionMenuOverhaul;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustHotbars : MonoBehaviour
{
    public GameObject HotbarsGo;
    private Hotbars hotbars;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AdjustHotbars: Getting HotbarsMain component from HotbarsMainGo. HotbarsMainGo is null? " + HotbarsGo == null);
        hotbars = HotbarsGo.GetComponent<Hotbars>();
    }

    public void IncrementHotbars()
    {
        hotbars.ConfigureHotbars(hotbars.HotbarCount + 1, hotbars.ActionSlotsPerBar);
    }
    public void DecrementHotbars()
    {
        if (hotbars.HotbarCount > 1)
            hotbars.ConfigureHotbars(hotbars.HotbarCount - 1, hotbars.ActionSlotsPerBar);
    }
    public void IncrementActionSlots()
    {
        hotbars.ConfigureHotbars(hotbars.HotbarCount, hotbars.ActionSlotsPerBar + 1);
    }
    public void DecrementActionSlots()
    {
        if (hotbars.ActionSlotsPerBar > 8)
            hotbars.ConfigureHotbars(hotbars.HotbarCount, hotbars.ActionSlotsPerBar - 1);
    }
}
