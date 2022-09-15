using ModifAmorphic.Outward.Unity.ActionMenus.Extensions;
using ModifAmorphic.Outward.Unity.ActionUI;
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
        ProfileName,
        HotkeyCapture,
        UIPosition
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

        public Toggle SettingsViewToggle;
        public Toggle HotbarViewToggle;

        public bool IsShowing => gameObject.activeSelf || HotkeyCaptureMenu.IsShowing || ProfileInput.IsShowing || UIPositionScreen.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private Dictionary<ActionSettingsMenus, ISettingsView> _menus;

        private bool _isInit;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            DebugLogger.Log("MainSettingsMenu::Awake");
            _menus = new Dictionary<ActionSettingsMenus, ISettingsView>();
            _menus.Add(ActionSettingsMenus.Settings, SettingsView);
            _menus.Add(ActionSettingsMenus.ActionSlots, HotbarSettingsView);
            _menus.Add(ActionSettingsMenus.ProfileName, ProfileInput);
            _menus.Add(ActionSettingsMenus.HotkeyCapture, HotkeyCaptureMenu);
            _menus.Add(ActionSettingsMenus.UIPosition, UIPositionScreen);

            SettingsViewToggle.onValueChanged.AddListener(isOn => ShowMenu(ActionSettingsMenus.Settings));
            HotbarViewToggle.onValueChanged.AddListener(isOn => ShowMenu(ActionSettingsMenus.ActionSlots));
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
                HotbarSettingsView.Show();
            else
                SettingsView.Show();
        }
        public void Hide() => Hide(true);

        private void Hide(bool raiseEvent)
        {
            DebugLogger.Log("MainSettingsMenu::Hide");

            var hideMenus = _menus
                .Where(kvp => kvp.Value.IsShowing && kvp.Key != ActionSettingsMenus.ActionSlots && kvp.Key != ActionSettingsMenus.Settings)
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
                menu.Hide();
        }
    }
}