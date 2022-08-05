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

        public Button SetHotkeys;

        public HotbarsContainer Hotbars;
        public IHotbarController HotbarsController;

        private IHotbarProfileData _activeProfile => GetProfileData().GetActiveProfile();

        private Func<bool> _exitRequested;
        public bool IsShowing => gameObject.activeSelf;

        private void Awake()
        {
            Hide();
        }

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
            BarAmountInput.SetAmount(_activeProfile.Hotbars.Count);
            RowAmountInput.SetAmount(_activeProfile.Rows);
            SlotAmountInput.SetAmount(_activeProfile.SlotsPerRow);

            ProfileDropdown.ClearOptions();
            var profiles = GetProfileData().GetProfileNames();
            var profileOptions = profiles.Select(p => new Dropdown.OptionData(p)).ToList();
            profileOptions.Add(new Dropdown.OptionData("[New Profile]"));
            ProfileDropdown.AddOptions(profileOptions);
            ProfileDropdown.value = profileOptions.FindIndex(o => o.text.Equals(_activeProfile.Name, System.StringComparison.InvariantCultureIgnoreCase));

        }
        private void HookControls()
        {
            //HotbarsController = Hotbars.Controller;
            //var config = _currentProfile.Hotbars.First().SlotsAssigned.First().Config;
            BarAmountInput.OnValueChanged.AddListener(amount =>
            {
                if (_activeProfile.Hotbars.Count < amount)
                    GetProfileData().AddHotbar(_activeProfile);
                else if(_activeProfile.Hotbars.Count > amount)
                    GetProfileData().RemoveHotbar(_activeProfile);
            });
            //BarAmountInput.InputText
            //    .onValueChanged.AddListener((call) => _currentProfile = GetProfileData().AddHotbar(_currentProfile.Name));

            //BarAmountInput.InputText
            //    .onValueChanged.AddListener((call) => OnBarsChanged.Invoke(BarAmountInput.Amount));
            //BarAmountInput.InputText
            //    .onValueChanged.AddListener((call) => HotbarsController.ConfigureHotbars(
            //        BarAmountInput.Amount,
            //        RowAmountInput.Amount,
            //        SlotAmountInput.Amount,
            //        GetActionSlotConfigs(BarAmountInput.Amount, RowAmountInput.Amount, SlotAmountInput.Amount, config)));

            //RowAmountInput.InputText
            //    .onValueChanged.AddListener((call) => OnRowsChanged.Invoke(RowAmountInput.Amount));
            //RowAmountInput.InputText
            //    .onValueChanged.AddListener((call) => HotbarsController.ConfigureHotbars(
            //        BarAmountInput.Amount,
            //        RowAmountInput.Amount,
            //        SlotAmountInput.Amount,
            //        GetActionSlotConfigs(BarAmountInput.Amount, RowAmountInput.Amount, SlotAmountInput.Amount, config)));

            //SlotAmountInput.InputText
            //    .onValueChanged.AddListener((call) => OnSlotsChanged.Invoke(SlotAmountInput.Amount));
            //SlotAmountInput.InputText
            //    .onValueChanged.AddListener((call) => HotbarsController.ConfigureHotbars(
            //        BarAmountInput.Amount,
            //        RowAmountInput.Amount,
            //        SlotAmountInput.Amount,
            //        GetActionSlotConfigs(BarAmountInput.Amount, RowAmountInput.Amount, SlotAmountInput.Amount, config)));
        }
        private IHotbarProfileDataService GetProfileData() => Psp.GetServicesProvider(PlayerMenu.PlayerID).GetService<IHotbarProfileDataService>();
    }
}