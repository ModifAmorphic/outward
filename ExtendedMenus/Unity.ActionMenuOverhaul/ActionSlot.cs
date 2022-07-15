using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenuOverhaul
{
    [UnityScriptComponent(ComponentPath = "OverhaulCanvas/HotbarsMain/HotbarsPanel/ActionSlot")]
    public class ActionSlot : MonoBehaviour
    {
        private Text buttonText;
        public int HotbarId;
        public int SlotNo;

        void Awake()
        {
            if (SlotNo != 0)
            {
                name = $"ActionSlot_{HotbarId}_{SlotNo - 1}";
                buttonText = GetComponentInChildren<Text>();
                buttonText.text = $"ActionSlot: {SlotNo}"; //itemTypes[typeIndex];
            }
        }
    }
}