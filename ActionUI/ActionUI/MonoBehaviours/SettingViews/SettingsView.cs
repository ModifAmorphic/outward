using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class SettingsView : MonoBehaviour, ISettingsView
    {
        public MainSettingsMenu MainSettingsMenu;

        public Dropdown ProfileDropdown;
        public Button ProfileRenameButton;

        public Toggle ActionSlotsToggle;
        public Toggle DurabilityToggle;
        public Toggle EquipmentSetsToggle;
        public Toggle StashCraftingToggle;
        public Toggle CraftingOutsideTownsToggle;

        public Button MoveUIButton;
        public Button ResetUIButton;

        public bool IsShowing => gameObject.activeSelf && !MainSettingsMenu.ProfileInput.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private IActionUIProfile _profile => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService.GetActiveProfile();

        private IActionUIProfileService _profileService => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService;

        private SelectableTransitions[] _selectables;
        private Selectable _lastSelected;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _selectables = GetComponentsInChildren<SelectableTransitions>();

            for (int i = 0; i < _selectables.Length; i++)
            {
                _selectables[i].OnSelected += SettingSelected;
                _selectables[i].OnDeselected += SettingDeselected;
            }

            Hide();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            SetControls();
            HookControls();
        }

        public void Show()
        {
            DebugLogger.Log("SettingsView::Show");
            gameObject.SetActive(true);
            SetControls();
            StartCoroutine(SelectNextFrame(MainSettingsMenu.SettingsViewToggle));

            //if (_selectables != null && _selectables.Any())
            //{
            //    _selectables.First().Selectable.Select();
            //    EventSystem.current.SetSelectedGameObject(_selectables.First().gameObject, MainSettingsMenu.PlayerMenu.PlayerID);
            //}
            //else
            //{

            //    MainSettingsMenu.SettingsViewToggle.Select();
            //    EventSystem.current.SetSelectedGameObject(MainSettingsMenu.SettingsViewToggle.gameObject, MainSettingsMenu.PlayerMenu.PlayerID);

            //}

            OnShow?.Invoke();
        }
        public void Hide()
        {
            DebugLogger.Log("SettingsView::Hide");
            if (MainSettingsMenu.ProfileInput.IsShowing)
            {
                MainSettingsMenu.ProfileInput.Hide();
                return;
            }
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }
        private void SetControls()
        {
            SetProfiles();

            ActionSlotsToggle.SetIsOn(_profile?.ActionSlotsEnabled ?? false);
            DurabilityToggle.SetIsOn(_profile?.DurabilityDisplayEnabled ?? false);
            EquipmentSetsToggle.SetIsOn(_profile?.EquipmentSetsEnabled ?? false);
            StashCraftingToggle.SetIsOn(_profile?.StashCraftingEnabled ?? false);
            CraftingOutsideTownsToggle.SetIsOn(_profile?.CraftingOutsideTownEnabled ?? false);
        }
        private void HookControls()
        {
            ProfileDropdown.onValueChanged.AddListener(SelectProfile);
            ProfileRenameButton.onClick.AddListener(RenameProfile);
            MainSettingsMenu.ProfileInput.OnHide.AddListener(() =>
            {
                ProfileDropdown.Select();
                SetProfiles();
            });

            ActionSlotsToggle.onValueChanged.AddListener(isOn =>
            {
                _profile.ActionSlotsEnabled = isOn;
                DebugLogger.Log($"Before ProfileService: isOn == {isOn}; ActionSlotsEnabled == {_profile.ActionSlotsEnabled}");
                var profileService = _profileService;
                DebugLogger.Log($"Before Save: isOn == {isOn}; ActionSlotsEnabled == {_profile.ActionSlotsEnabled}");
                profileService.Save();
                DebugLogger.Log($"After Save: isOn == {isOn}; ActionSlotsEnabled == {_profile.ActionSlotsEnabled}");
            });

            DurabilityToggle.onValueChanged.AddListener(isOn =>
            {
                DebugLogger.Log($"DurabilityToggle changed from {!isOn} to {isOn}. Saving profile.");
                _profile.DurabilityDisplayEnabled = isOn;
                _profileService.Save();
            });

            EquipmentSetsToggle.onValueChanged.AddListener(isOn =>
            {
                DebugLogger.Log($"EquipmentSetsToggle changed from {!isOn} to {isOn}. Saving profile.");
                _profile.EquipmentSetsEnabled = isOn;
                _profileService.Save();
            });

            StashCraftingToggle.onValueChanged.AddListener(isOn =>
            {
                DebugLogger.Log($"StashCraftingToggle changed from {!isOn} to {isOn}. Saving profile.");
                _profile.StashCraftingEnabled = isOn;
                _profileService.Save();
            });

            CraftingOutsideTownsToggle.onValueChanged.AddListener(isOn =>
            {
                DebugLogger.Log($"CraftingOutsideTownsToggle changed from {!isOn} to {isOn}. Saving profile.");
                _profile.CraftingOutsideTownEnabled = isOn;
                _profileService.Save();
            });

            MoveUIButton.onClick.AddListener(ShowPositionScreen);
            ResetUIButton.onClick.AddListener(ResetUIPositions);
        }

        private void SetProfiles()
        {
            ProfileDropdown.ClearOptions();
            var profiles = MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService.GetProfileNames();
            var profileOptions = profiles.OrderBy(p => p).Select(p => new Dropdown.OptionData(p)).ToList();
            profileOptions.Add(new Dropdown.OptionData("[New Profile]"));
            ProfileDropdown.AddOptionSilent(profileOptions);
            ProfileDropdown.SetValue(profileOptions.FindIndex(o => o.text.Equals(_profile.Name, StringComparison.InvariantCultureIgnoreCase)));
        }
        private void SelectProfile(int profileIndex)
        {

            if (profileIndex < ProfileDropdown.options.Count - 1)
            {
                MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService.SetActiveProfile(ProfileDropdown.options[profileIndex].text);
                SetControls();
            }
            else
            {
                MainSettingsMenu.ProfileInput.Show();
            }
        }

        private void RenameProfile()
        {
            MainSettingsMenu.ProfileInput.Show(MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService.GetActiveProfile().Name);
        }

        private void ShowPositionScreen()
        {
            MainSettingsMenu.gameObject.SetActive(false);
            MainSettingsMenu.UIPositionScreen.Show();

        }
        private void ResetUIPositions()
        {
            var uis = MainSettingsMenu.PlayerMenu.GetPositionableUIs();

            var positonService = MainSettingsMenu.PlayerMenu.ProfileManager.PositionsProfileService;
            var positions = positonService.GetProfile();
            bool saveNeeded = false;

            DebugLogger.Log($"Checking {uis.Length} UI Element positons for changes.");

            foreach (var ui in uis)
            {
                ui.ResetToOrigin();
                var uiPositons = ui.GetUIPositions();
                if (ui.HasMoved)
                {
                    positions.AddOrReplacePosition(ui.GetUIPositions());
                    saveNeeded = true;
                }
            }

            if (saveNeeded)
                positonService.Save();

        }

        private void SettingSelected(SelectableTransitions transition)
        {
            DebugLogger.Log($"{transition.name} Selected.");
            _lastSelected = transition.Selectable;
        }

        private void SettingDeselected(SelectableTransitions transition)
        {
            DebugLogger.Log($"{transition.name} Deselected.");
            //if (_selectables.Any(s => s.Selected) || !gameObject.activeSelf)
            //    return;

            StartCoroutine(CheckSetSelectedSetting());
        }

        private IEnumerator CheckSetSelectedSetting()
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            if (!_selectables.Any(s => s.Selected) && !MainSettingsMenu.MenuItemSelected && gameObject.activeSelf && _lastSelected != null)
            {
                _lastSelected.Select();
            }

        }
        private IEnumerator SelectNextFrame(Selectable selectable)
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            selectable.Select();
        }
    }
}