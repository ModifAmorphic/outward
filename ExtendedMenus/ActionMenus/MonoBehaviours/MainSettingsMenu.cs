using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionMenus.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public enum ActionSettingsMenus
    {
        Settings,
        ActionSlots,
        NewProfile,
        HotkeyCapture,
        UIPosition
    }
    [UnityScriptComponent]
    public class MainSettingsMenu : MonoBehaviour, IActionMenu
    {
        public PlayerActionMenus PlayerMenu;
        public SettingsView SettingsView;
        public NewProfileInput NewProfileInput;
        public HotbarSettingsView HotbarSettingsView;
        public HotkeyCaptureMenu HotkeyCaptureMenu;
        public UIPositionScreen UIPositionScreen;

        public Toggle SettingsViewToggle;
        public Toggle HotbarViewToggle;

        public bool IsShowing => gameObject.activeSelf || HotkeyCaptureMenu.IsShowing || NewProfileInput.IsShowing || UIPositionScreen.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private Dictionary<ActionSettingsMenus, ISettingsView> _menus;

        private bool _isInit;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            Debug.Log("MainSettingsViewer::Awake");
            _menus = new Dictionary<ActionSettingsMenus, ISettingsView>();
            _menus.Add(ActionSettingsMenus.Settings, SettingsView);
            _menus.Add(ActionSettingsMenus.ActionSlots, HotbarSettingsView);
            _menus.Add(ActionSettingsMenus.NewProfile, NewProfileInput);
            _menus.Add(ActionSettingsMenus.HotkeyCapture, HotkeyCaptureMenu);
            _menus.Add(ActionSettingsMenus.UIPosition, UIPositionScreen);

            SettingsViewToggle.onValueChanged.AddListener(isOn => ShowMenu(ActionSettingsMenus.Settings));
            HotbarViewToggle.onValueChanged.AddListener(isOn => ShowMenu(ActionSettingsMenus.ActionSlots));

            //Hide(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            Debug.Log("MainSettingsViewer::Start");
            if (!_isInit)
                Hide(false);

            _isInit = true;
        }

        public void Show()
        {
            Debug.Log("MainSettingsViewer::Show");
            gameObject.SetActive(true);

            OnShow?.TryInvoke();

            if (HotbarViewToggle.isOn)
                HotbarSettingsView.Show();
            else
                SettingsView.Show();
        }
        public void Hide() => Hide(true);

        private void Hide(bool raiseEvent)
        {
            Debug.Log("MainSettingsViewer::Hide");

            var hideMenus = _menus
                .Where(kvp => kvp.Value.IsShowing && kvp.Key != ActionSettingsMenus.ActionSlots && kvp.Key != ActionSettingsMenus.Settings)
                .Select(kvp => kvp.Value);
            
            bool showMe = false;
            foreach (var menu in hideMenus)
            {
                menu.Hide();
                showMe = true;
            }
            Debug.Log($"MainSettingsViewer::Hide: hideMenus.Count == {hideMenus?.Count()}");
            if (showMe)
            {
                Show();
            }
            else if (gameObject.activeSelf)
            {
                Debug.Log($"MainSettingsViewer::Hide: SetActive(false)");
                gameObject.SetActive(false);
                if (raiseEvent)
                    OnHide?.TryInvoke();
                return;
            }
        }

        private void ShowMenu(ActionSettingsMenus menuType)
        {
            _menus[menuType].Show();
            var hideMenus = _menus.Where(kvp => kvp.Key != menuType).Select(kvp => kvp.Value);
            foreach (var menu in hideMenus)
                menu.Hide();
        }
    }
}