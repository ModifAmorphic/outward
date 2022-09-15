using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class HotbarSettingsView : MonoBehaviour, ISettingsView
    {
        public UnityEvent<int> OnBarsChanged;
        public UnityEvent<int> OnRowsChanged;
        public UnityEvent<int> OnSlotsChanged;

        public MainSettingsMenu MainSettingsMenu;
        public HotkeyCaptureMenu HotkeyCaptureMenu;

        public ArrowInput BarAmountInput;
        public ArrowInput RowAmountInput;
        public ArrowInput SlotAmountInput;

        public Toggle ShowCooldownTimer;
        public Toggle ShowPrecisionTime;
        public Toggle CombatMode;

        public Dropdown EmptySlotDropdown;

        public Button SetHotkeys;

        public HotbarsContainer Hotbars;


        public bool IsShowing => gameObject.activeSelf;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private IHotbarProfile _hotbarProfile => MainSettingsMenu.PlayerMenu.ProfileManager.HotbarProfileService.GetProfile();

        private IHotbarProfileService _hotbarService => MainSettingsMenu.PlayerMenu.ProfileManager.HotbarProfileService;

        private bool _eventsAdded;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            SetHotkeys.onClick.AddListener(EnableHotkeyEdits);
            Hide(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            if (!_eventsAdded)
            {
                HookControls();
                _eventsAdded = true;
            }
        }

        public void Show()
        {
            DebugLogger.Log("HotbarSettingsViewer::Show");
            gameObject.SetActive(true);

            SetControls();
            OnShow?.Invoke();
        }

        public void Hide() => Hide(true);

        private void Hide(bool raiseEvent)
        {
            DebugLogger.Log("HotbarSettingsViewer::Hide");
            gameObject.SetActive(false);
            if (raiseEvent)
                OnHide?.Invoke();
        }
        private void SetControls()
        {
            var config = _hotbarProfile.Hotbars.First().Slots.First().Config;

            BarAmountInput.SetAmount(_hotbarProfile.Hotbars.Count);
            RowAmountInput.SetAmount(_hotbarProfile.Rows);
            SlotAmountInput.SetAmount(_hotbarProfile.SlotsPerRow);

            ShowCooldownTimer.isOn = config?.ShowCooldownTime ?? false;
            ShowPrecisionTime.isOn = config?.PreciseCooldownTime ?? false;
            CombatMode.isOn = _hotbarProfile.CombatMode;

            EmptySlotDropdown.ClearOptions();
            var imageOptions = Enum.GetNames(typeof(EmptySlotOptions)).Select(name => new Dropdown.OptionData(name)).ToList();
            EmptySlotDropdown.AddOptions(imageOptions);
            var selectedName = Enum.GetName(typeof(EmptySlotOptions), config?.EmptySlotOption ?? EmptySlotOptions.Image);
            EmptySlotDropdown.value = imageOptions.FindIndex(o => o.text.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));

        }
        private void HookControls()
        {
            BarAmountInput.OnValueChanged.AddListener(amount =>
            {
                if (_hotbarProfile.Hotbars.Count < amount)
                    _hotbarService.AddHotbar();
                else if (_hotbarProfile.Hotbars.Count > amount)
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
            HotkeyCaptureMenu.Show();
            MainSettingsMenu.gameObject.SetActive(false);
        }
    }
}