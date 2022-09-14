using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public Button MoveUIButton;
        public Button ResetUIButton;

        public bool IsShowing => gameObject.activeSelf && !MainSettingsMenu.ProfileInput.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private IActionMenusProfile _profile => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService.GetActiveProfile();

        private IActionMenusProfileService _profileService => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService;

        private bool _settingProfiles;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
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
            Debug.Log("SettingsView::Show");
            gameObject.SetActive(true);
            SetControls();

            OnShow?.Invoke();
        }
        public void Hide()
        {
            Debug.Log("SettingsView::Hide");
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

            ActionSlotsToggle.isOn = _profile?.ActionSlotsEnabled ?? false;
            DurabilityToggle.isOn = _profile?.DurabilityDisplayEnabled ?? false;
        }
        private void HookControls()
        {
            ProfileDropdown.onValueChanged.AddListener(SelectProfile);
            ProfileRenameButton.onClick.AddListener(RenameProfile);
            MainSettingsMenu.ProfileInput.OnHide.AddListener(SetProfiles);
            
            ActionSlotsToggle.onValueChanged.AddListener(isOn =>
            {
                _profile.ActionSlotsEnabled = isOn;
                Debug.Log($"Before ProfileService: isOn == {isOn}; ActionSlotsEnabled == {_profile.ActionSlotsEnabled}");
                var profileService = _profileService;
                Debug.Log($"Before Save: isOn == {isOn}; ActionSlotsEnabled == {_profile.ActionSlotsEnabled}");
                profileService.Save();
                Debug.Log($"After Save: isOn == {isOn}; ActionSlotsEnabled == {_profile.ActionSlotsEnabled}");
            });

            DurabilityToggle.onValueChanged.AddListener(isOn =>
            {
                _profile.DurabilityDisplayEnabled = isOn;
                _profileService.Save();
            });

            
            MoveUIButton.onClick.AddListener(ShowPositionScreen);
            ResetUIButton.onClick.AddListener(ResetUIPositions);
        }
        
        private void SetProfiles()
        {
            _settingProfiles = true;
            ProfileDropdown.ClearOptions();
            var profiles = MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService.GetProfileNames();
            var profileOptions = profiles.OrderBy(p => p).Select(p => new Dropdown.OptionData(p)).ToList();
            profileOptions.Add(new Dropdown.OptionData("[New Profile]"));
            ProfileDropdown.AddOptions(profileOptions);
            ProfileDropdown.value = profileOptions.FindIndex(o => o.text.Equals(_profile.Name, StringComparison.InvariantCultureIgnoreCase));
            _settingProfiles = false;
        }
        private void SelectProfile(int profileIndex)
        {
            if (_settingProfiles)
                return;

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

            Debug.Log($"Checking {uis.Length} UI Element positons for changes.");

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
    }
}