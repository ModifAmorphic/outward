using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using System;
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
        EquipmentSets,
        ProfileName,
        HotkeyCapture,
        UIPosition,
        Stash
    }
    [UnityScriptComponent]
    public class MainSettingsMenu : MonoBehaviour, IActionMenu
    {
        public PlayerActionMenus PlayerMenu;
        public SettingsView SettingsView;
        public ProfileInput ProfileInput;
        public HotbarSettingsView HotbarSettingsView;
        public HotkeyCaptureMenu HotkeyCaptureMenu;
        public UIPositionScreen UIPositionScreen;
        public EquipmentSetsSettingsView EquipmentSetsSettingsView;
        public StorageSettingsView StorageSettingsView;

        public Toggle SettingsViewToggle;
        public Toggle HotbarViewToggle;
        public Toggle EquipmentSetViewToggle;
        public Toggle StashViewToggle;

        public bool IsShowing => gameObject.activeSelf || HotkeyCaptureMenu.IsShowing || ProfileInput.IsShowing || UIPositionScreen.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private Dictionary<ActionSettingsMenus, ISettingsView> _menus;

        private Dictionary<ActionSettingsMenus, ISettingsView> _settingsViews;

        private SelectableTransitions[] _selectables;
        public bool MenuItemSelected => (_selectables != null && _selectables.Any(s => s.Selected) || ProfileInput.gameObject.activeSelf);

        private bool _isInit;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            DebugLogger.Log("MainSettingsMenu::Awake");
            _menus = new Dictionary<ActionSettingsMenus, ISettingsView>();
            _menus.Add(ActionSettingsMenus.Settings, SettingsView);
            _menus.Add(ActionSettingsMenus.ActionSlots, HotbarSettingsView);
            _menus.Add(ActionSettingsMenus.EquipmentSets, EquipmentSetsSettingsView);
            _menus.Add(ActionSettingsMenus.ProfileName, ProfileInput);
            _menus.Add(ActionSettingsMenus.HotkeyCapture, HotkeyCaptureMenu);
            _menus.Add(ActionSettingsMenus.UIPosition, UIPositionScreen);
            _menus.Add(ActionSettingsMenus.Stash, StorageSettingsView);

            _settingsViews = new Dictionary<ActionSettingsMenus, ISettingsView>()
            {
                { ActionSettingsMenus.Settings, SettingsView },
                { ActionSettingsMenus.ActionSlots, HotbarSettingsView },
                { ActionSettingsMenus.EquipmentSets, EquipmentSetsSettingsView },
                { ActionSettingsMenus.Stash, StorageSettingsView },
            };

            SettingsViewToggle.onValueChanged.AddListener(isOn => ShowMenu(ActionSettingsMenus.Settings));
            HotbarViewToggle.onValueChanged.AddListener(isOn => ShowMenu(ActionSettingsMenus.ActionSlots));
            EquipmentSetViewToggle.onValueChanged.AddListener(isOn => ShowMenu(ActionSettingsMenus.EquipmentSets));
            StashViewToggle.onValueChanged.AddListener(isOn => ShowMenu(ActionSettingsMenus.Stash));

            _selectables = GetComponentsInChildren<SelectableTransitions>();
            //_selectables = transform.Find("MenuToggles").GetComponentsInChildren<SelectableTransitions>();
            //for (int i = 0; i < _selectables.Length; i++)
            //{
            //    _selectables[i].OnSelected += SettingSelected;
            //    _selectables[i].OnDeselected += SettingDeselected;
            //}
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            DebugLogger.Log("MainSettingsMenu::Start");
            if (!_isInit)
                Hide(false);

            _isInit = true;
        }

        public void Show()
        {
            DebugLogger.Log("MainSettingsMenu::Show");
            gameObject.SetActive(true);

            OnShow?.TryInvoke();

            if (HotbarViewToggle.isOn)
                ShowMenu(ActionSettingsMenus.ActionSlots);
            else if (EquipmentSetViewToggle.isOn)
                ShowMenu(ActionSettingsMenus.EquipmentSets);
            else if (StashViewToggle.isOn)
                ShowMenu(ActionSettingsMenus.Stash);
            else
                ShowMenu(ActionSettingsMenus.Settings);
        }
        public void Hide() => Hide(true);

        private void Hide(bool raiseEvent)
        {
            DebugLogger.Log("MainSettingsMenu::Hide");

            var hideMenus = _menus
                .Where(kvp => kvp.Value.IsShowing && !_settingsViews.ContainsKey(kvp.Key))
                .Select(kvp => kvp.Value);

            bool showMe = false;
            foreach (var menu in hideMenus)
            {
                menu.Hide();
                showMe = true;
            }
            DebugLogger.Log($"MainSettingsMenu::Hide: hideMenus.Count == {hideMenus?.Count()}");
            if (showMe)
            {
                Show();
            }
            else if (gameObject.activeSelf)
            {
                DebugLogger.Log($"MainSettingsMenu::Hide: SetActive(false)");
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
            {
                try
                {
                    menu.Hide();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to hide menu type {menuType}. Disabling menu.");
                    Debug.LogException(ex);
                    if (menu is MonoBehaviour behaviour)
                    {
                        behaviour.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}