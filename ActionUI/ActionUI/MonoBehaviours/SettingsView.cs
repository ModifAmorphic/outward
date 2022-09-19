using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using System;
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
        public Toggle StashCraftingToggle;

        public Button MoveUIButton;
        public Button ResetUIButton;

        public bool IsShowing => gameObject.activeSelf && !MainSettingsMenu.ProfileInput.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private IActionUIProfile _profile => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService.GetActiveProfile();

        private IActionUIProfileService _profileService => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService;

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
            DebugLogger.Log("SettingsView::Show");
            gameObject.SetActive(true);
            SetControls();

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
            StashCraftingToggle.SetIsOn(_profile?.StashCraftingEnabled ?? false);
        }
        private void HookControls()
        {
            ProfileDropdown.onValueChanged.AddListener(SelectProfile);
            ProfileRenameButton.onClick.AddListener(RenameProfile);
            MainSettingsMenu.ProfileInput.OnHide.AddListener(SetProfiles);

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

            StashCraftingToggle.onValueChanged.AddListener(isOn =>
            {
                DebugLogger.Log($"StashCraftingToggle changed from {!isOn} to {isOn}. Saving profile.");
                _profile.StashCraftingEnabled = isOn;
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
    }
}