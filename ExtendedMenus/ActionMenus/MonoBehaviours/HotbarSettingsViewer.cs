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
    public class HotbarSettingsViewer : MonoBehaviour
    {
        public UnityEvent<int> OnBarsChanged;
        public UnityEvent<int> OnRowsChanged;
        public UnityEvent<int> OnSlotsChanged;

        public PlayerMenu PlayerMenu;

        public Dropdown ProfileDropdown;

        public ArrowInput BarAmountInput;
        public ArrowInput RowAmountInput;
        public ArrowInput SlotAmountInput;

        public Toggle ShowCooldownTimer;
        public Toggle ShowPrecisionTime;

        public Dropdown EmptySlotDropdown;

        public Button SetHotkeys;

        public HotbarsContainer Hotbars;
        public IHotbarController HotbarsController;

        private IHotbarProfileData _activeProfile => GetProfileData().GetActiveProfile();

        private Func<bool> _exitRequested;
        public bool IsShowing => gameObject.activeSelf;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            Hide();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Update()
        {
            if (_exitRequested != null && IsShowing && _exitRequested.Invoke())
                Hide();
        }

        public void ConfigureExit(Func<bool> exitRequested) => _exitRequested = exitRequested;

        public void Show()
        {
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
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        private void SetControls()
        {
            var config = _activeProfile.Hotbars.First().Slots.First().Config;

            BarAmountInput.SetAmount(_activeProfile.Hotbars.Count);
            RowAmountInput.SetAmount(_activeProfile.Rows);
            SlotAmountInput.SetAmount(_activeProfile.SlotsPerRow);

            ProfileDropdown.ClearOptions();
            var profiles = GetProfileData().GetProfileNames();
            var profileOptions = profiles.OrderBy(p => p).Select(p => new Dropdown.OptionData(p)).ToList();
            profileOptions.Add(new Dropdown.OptionData("[New Profile]"));
            ProfileDropdown.AddOptions(profileOptions);
            ProfileDropdown.value = profileOptions.FindIndex(o => o.text.Equals(_activeProfile.Name, System.StringComparison.InvariantCultureIgnoreCase));

            ShowCooldownTimer.isOn = config.ShowCooldownTime;
            ShowPrecisionTime.isOn = config.PreciseCooldownTime;

            EmptySlotDropdown.ClearOptions();
            var imageOptions = Enum.GetNames(typeof(EmptySlotOptions)).Select(name => new Dropdown.OptionData(name)).ToList();
            EmptySlotDropdown.AddOptions(imageOptions);
            var selectedName = Enum.GetName(typeof(EmptySlotOptions), config.EmptySlotOption);
            EmptySlotDropdown.value = imageOptions.FindIndex(o => o.text.Equals(selectedName, System.StringComparison.InvariantCultureIgnoreCase));

        }
        private void HookControls()
        {
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

            EmptySlotDropdown.onValueChanged.AddListener(value =>
                GetProfileData().SetEmptySlotView(_activeProfile, (EmptySlotOptions)value)
            );
        }
        private IHotbarProfileDataService GetProfileData() => Psp.Instance.GetServicesProvider(PlayerMenu.PlayerID).GetService<IHotbarProfileDataService>();
    }
}