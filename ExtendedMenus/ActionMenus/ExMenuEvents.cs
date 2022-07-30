using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionMenus
{
    public class ExMenuEvents : MonoBehaviour
    {
        public static ExMenuEvents Instance;
        public Button[] MenuButtons;
        public event Action<Button> OnClick;

        void Start()
        {
            Instance = this;
            UnityEngine.Debug.Log("ExMenuEvents::Start() Hooking up ButtonClick events.");
            MenuButtons = this.GetComponentsInChildren<Button>();
            for (int i = 0; i < MenuButtons.Length; i++)
            {
                var btn = MenuButtons[i];
                Debug.Log("ExMenuEvents: Attaching listener to onclick for Button " + btn.name + ".");
                btn.onClick.AddListener(() => OnClick?.Invoke(btn));
            }
            OnClick += InternalListener;
        }

        private void InternalListener(Button button)
        {
            UnityEngine.Debug.Log("Button " + button.name + " clicked!");
        }
    }
}