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


        public bool IsShowing => gameObject.activeSelf && !NewProfileInput.IsShowing;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private IHotbarProfile _hotbarProfile => PlayerMenu.ProfileManager.HotbarProfileService.GetProfile();

        private IHotbarProfileService _hotbarService => PlayerMenu.ProfileManager.HotbarProfileService;

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
            var config = _hotbarProfile.Hotbars.First().Slots.First().Config;

            BarAmountInput.SetAmount(_hotbarProfile.Hotbars.Count);
            RowAmountInput.SetAmount(_hotbarProfile.Rows);
            SlotAmountInput.SetAmount(_hotbarProfile.SlotsPerRow);

            SetProfiles();

            ShowCooldownTimer.isOn = config?.ShowCooldownTime ?? false;
            ShowPrecisionTime.isOn = config?.PreciseCooldownTime ?? false;
            CombatMode.isOn = _hotbarProfile.CombatMode;

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
                if (_hotbarProfile.Hotbars.Count < amount)
                    _hotbarService.AddHotbar();
                else if(_hotbarProfile.Hotbars.Count > amount)
                    _hotbarService.RemoveHotbar();
            });

            RowAmountInput.OnValueChanged.AddListener(amount =>
            {
                if (_hotbarProfile.Rows < amount)
                    _hotbarService.AddRow();
                else if (_hotbarProfile.Rows > amount)
                    _hotbarService.RemoveRow();
            });

            SlotAmountInput.OnValueChanged.AddListener(amount =>
            {
                if (_hotbarProfile.SlotsPerRow < amount)
                    _hotbarService.AddSlot();
                else if (_hotbarProfile.SlotsPerRow > amount)
                    _hotbarService.RemoveSlot();
            });

            ShowCooldownTimer.onValueChanged.AddListener(isOn =>
                _hotbarService.SetCooldownTimer(ShowCooldownTimer.isOn, ShowPrecisionTime.isOn)
            );
            ShowPrecisionTime.onValueChanged.AddListener(isOn =>
                _hotbarService.SetCooldownTimer(ShowCooldownTimer.isOn, ShowPrecisionTime.isOn)
            );
            CombatMode.onValueChanged.AddListener(isOn =>
                _hotbarService.SetCombatMode(CombatMode.isOn)
            );

            EmptySlotDropdown.onValueChanged.AddListener(value =>
                _hotbarService.SetEmptySlotView((EmptySlotOptions)value)
            );
        }

        private void EnableHotkeyEdits()
        {
            Hotbars.Controller.ToggleActionSlotEdits(true);
            PlayerMenu.HotkeyCaptureMenu.Show();
            Hide();
        }
        private void SetProfiles()
        {
            ProfileDropdown.ClearOptions();
            var profiles = PlayerMenu.ProfileManager.GetProfileNames();
            var profileOptions = profiles.OrderBy(p => p).Select(p => new Dropdown.OptionData(p)).ToList();
            profileOptions.Add(new Dropdown.OptionData("[New Profile]"));
            ProfileDropdown.AddOptions(profileOptions);
            ProfileDropdown.value = profileOptions.FindIndex(o => o.text.Equals(_hotbarProfile.Name, System.StringComparison.InvariantCultureIgnoreCase));
        }
        private void SelectProfile(int profileIndex)
        {
            if (profileIndex < ProfileDropdown.options.Count - 1)
            {
                PlayerMenu.ProfileManager.SetActiveProfile(ProfileDropdown.options[profileIndex].text);
            }
            else
            {
                NewProfileInput.Show(_hotbarService);
            }
        }
    }
}