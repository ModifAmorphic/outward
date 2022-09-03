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
        public NewProfileInput NewProfileInput;
        public MainSettingsMenu MainSettingsMenu;

        public Dropdown ProfileDropdown;

        public Toggle ActionSlotsToggle;
        public Toggle DurabilityToggle;

        public Button MoveUIButton;

        public bool IsShowing => gameObject.activeSelf && !NewProfileInput.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private IActionMenusProfile _profile => MainSettingsMenu.PlayerMenu.ProfileManager.GetActiveProfile();

        private IActionMenusProfileService _profileService => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService;

        private Dictionary<ActionSettingsMenus, IActionMenu> _menus = new Dictionary<ActionSettingsMenus, IActionMenu>();

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

            OnShow?.Invoke();
        }
        public void Hide()
        {
            Debug.Log("SettingsView::Hide");
            if (NewProfileInput.IsShowing)
            {
                NewProfileInput.Hide();
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
            NewProfileInput.OnHide.AddListener(SetProfiles);
            
            ActionSlotsToggle.onValueChanged.AddListener(isOn =>
            {
                _profile.ActionSlotsEnabled = isOn;
                _profileService.Save();
            });

            DurabilityToggle.onValueChanged.AddListener(isOn =>
            {
                _profile.DurabilityDisplayEnabled = isOn;
                _profileService.Save();
            });

            MoveUIButton.onClick.AddListener(ShowPositionScreen);
        }

        private void SetProfiles()
        {
            ProfileDropdown.ClearOptions();
            var profiles = MainSettingsMenu.PlayerMenu.ProfileManager.GetProfileNames();
            var profileOptions = profiles.OrderBy(p => p).Select(p => new Dropdown.OptionData(p)).ToList();
            profileOptions.Add(new Dropdown.OptionData("[New Profile]"));
            ProfileDropdown.AddOptions(profileOptions);
            ProfileDropdown.value = profileOptions.FindIndex(o => o.text.Equals(_profile.Name, StringComparison.InvariantCultureIgnoreCase));
        }
        private void SelectProfile(int profileIndex)
        {
            if (profileIndex < ProfileDropdown.options.Count - 1)
            {
                MainSettingsMenu.PlayerMenu.ProfileManager.SetActiveProfile(ProfileDropdown.options[profileIndex].text);
            }
            else
            {
                NewProfileInput.Show();
            }
        }

        private void ShowPositionScreen()
        {
            MainSettingsMenu.gameObject.SetActive(false);
            MainSettingsMenu.UIPositionScreen.Show();
        }
        
    }
}