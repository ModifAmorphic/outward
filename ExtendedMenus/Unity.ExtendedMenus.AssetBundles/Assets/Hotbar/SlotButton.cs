using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotButton : MonoBehaviour
{
    private Text buttonText;
    public int HotbarId;
    public int SlotNo;

    void Awake()
    {
        if (SlotNo != 0)
        {
            name = $"Slot_{HotbarId}_{SlotNo - 1}";
            buttonText = GetComponentInChildren<Text>();
            buttonText.text = $"Slot: {SlotNo}"; //itemTypes[typeIndex];
        }
    }
}
