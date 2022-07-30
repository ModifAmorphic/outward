using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class MouseClickListener : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent OnLeftClick;
        public UnityEvent OnRightClick;
        public UnityEvent OnMiddleClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnLeftClick.Invoke();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick.Invoke();
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
                OnMiddleClick.Invoke();
            }
        }
    }
}
