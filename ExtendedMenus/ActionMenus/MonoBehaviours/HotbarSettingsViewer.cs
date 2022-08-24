using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
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
    public class HotbarSettingsViewer : MonoBehaviour, IActionMenu
    {
        public UnityEvent<int> OnBarsChanged;
        public UnityEvent<int> OnRowsChanged;
        public UnityEvent<int> OnSlotsChanged;

        public NewProfileInput NewProfileInput;
        public PlayerActionMenus PlayerMenu;

        public Dropdown ProfileDropdown;

        public ArrowInput BarAmountInput;
        public ArrowInput RowAmountInput;
        public ArrowInput SlotAmountInput;

        public Toggle ShowCooldownTimer;
        public Toggle ShowPrecisionTime;
        public Toggle CombatMode;

        public Dropdown EmptySlotDropdown;

        public Button SetHotkeys;

        public HotbarsContainer Hotbars;

        private IHotbarProfileData _activeProfile => GetProfileData().GetActiveProfile();

        public bool IsShowing => gameObject.activeSelf && !NewProfileInput.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            Hide();
            SetHotkeys.onClick.AddListener(EnableHotkeyEdits);
        }

        public void Show()
        {
            Debug.Log("HotbarSettingsViewer::Show");
            gameObject.SetActive(true);

            var activeProfile = GetProfileData().GetActiveProfile();

            if (activeProfile == null)
            {
                var names = GetProfileData().GetProfileNames();
                if (names != null && names.Any())
                {
                    activeProfile = GetProfileData().GetProfile(names.First());
                    GetProfileData().SetActiveProfile(activeProfile.Name);
                }
            }
            SetControls();
            HookControls();
            OnShow?.Invoke();
        }
        public void Hide()
        {
            Debug.Log("HotbarSettingsViewer::Hide");
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
            var config = _activeProfile.Hotbars.First().Slots.First().Config;

            BarAmountInput.SetAmount(_activeProfile.Hotbars.Count);
            RowAmountInput.SetAmount(_activeProfile.Rows);
            SlotAmountInput.SetAmount(_activeProfile.SlotsPerRow);

            SetProfiles();

            ShowCooldownTimer.isOn = config?.ShowCooldownTime ?? false;
            ShowPrecisionTime.isOn = config?.PreciseCooldownTime ?? false;
            CombatMode.isOn = _activeProfile.CombatMode;

            EmptySlotDropdown.ClearOptions();
            var imageOptions = Enum.GetNames(typeof(EmptySlotOptions)).Select(name => new Dropdown.OptionData(name)).ToList();
            EmptySlotDropdown.AddOptions(imageOptions);
            var selectedName = Enum.GetName(typeof(EmptySlotOptions), config?.EmptySlotOption ?? EmptySlotOptions.Image);
            EmptySlotDropdown.value = imageOptions.FindIndex(o => o.text.Equals(selectedName, System.StringComparison.InvariantCultureIgnoreCase));

        }
        private void HookControls()
        {
            ProfileDropdown.onValueChanged.AddListener(SelectProfile);
            
            NewProfileInput.OnHide.AddListener(SetProfiles);

            BarAmountInput.OnValueChanged.AddListener(amount =>
            {
                if (_activeProfile.Hotbars.Count < amount)
                    GetProfileData().AddHotbar(_activeProfile);
                else if(_activeProfile.Hotbars.Count > amount)
                    GetProfileData().RemoveHotbar(_activeProfile);
            });

            RowAmountInput.OnValueChanged.AddListener(amount =>
            {
                if (_activeProfile.Rows < amount)
                    GetProfileData().AddRow(_activeProfile);
                else if (_activeProfile.Rows > amount)
                    GetProfileData().RemoveRow(_activeProfile);
            });

            SlotAmountInput.OnValueChanged.AddListener(amount =>
            {
                if (_activeProfile.SlotsPerRow < amount)
                    GetProfileData().AddSlot(_activeProfile);
                else if (_activeProfile.SlotsPerRow > amount)
                    GetProfileData().RemoveSlot(_activeProfile);
            });

            ShowCooldownTimer.onValueChanged.AddListener(isOn =>
                GetProfileData().SetCooldownTimer(_activeProfile, ShowCooldownTimer.isOn, ShowPrecisionTime.isOn)
            );
            ShowPrecisionTime.onValueChanged.AddListener(isOn =>
                GetProfileData().SetCooldownTimer(_activeProfile, ShowCooldownTimer.isOn, ShowPrecisionTime.isOn)
            );
            CombatMode.onValueChanged.AddListener(isOn =>
                GetProfileData().SetCombatMode(_activeProfile, CombatMode.isOn)
            );

            EmptySlotDropdown.onValueChanged.AddListener(value =>
                GetProfileData().SetEmptySlotView(_activeProfile, (EmptySlotOptions)value)
            );
        }
        private IHotbarProfileDataService GetProfileData() => Psp.Instance.GetServicesProvider(PlayerMenu.PlayerID).GetService<IHotbarProfileDataService>();

        private void EnableHotkeyEdits()
        {
            Hotbars.Controller.ToggleActionSlotEdits(true);
            PlayerMenu.HotkeyCaptureMenu.Show();
            Hide();
        }
        private void SetProfiles()
        {
            ProfileDropdown.ClearOptions();
            var profiles = GetProfileData().GetProfileNames();
            var profileOptions = profiles.OrderBy(p => p).Select(p => new Dropdown.OptionData(p)).ToList();
            profileOptions.Add(new Dropdown.OptionData("[New Profile]"));
            ProfileDropdown.AddOptions(profileOptions);
            ProfileDropdown.value = profileOptions.FindIndex(o => o.text.Equals(_activeProfile.Name, System.StringComparison.InvariantCultureIgnoreCase));
        }
        private void SelectProfile(int profileIndex)
        {
            if (profileIndex < ProfileDropdown.options.Count - 1)
            {
                GetProfileData().SetActiveProfile(ProfileDropdown.options[profileIndex].text);
            }
            else
            {
                NewProfileInput.Show(GetProfileData());
            }
        }
    }
}