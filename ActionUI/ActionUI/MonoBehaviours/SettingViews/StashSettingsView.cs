using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class StashSettingsView : MonoBehaviour, ISettingsView
    {
        public MainSettingsMenu MainSettingsMenu;

        public Toggle StashInventoryToggle;
        public Toggle StashInventoryAnywhereToggle;

        public Toggle MerchantStashToggle;
        public Toggle MerchantStashAnywhereToggle;

        public Toggle CraftFromStashToggle;
        public Toggle CraftFromStashAnywhereToggle;

        public Toggle PreserveFoodToggle;
        public InputField PreserveFoodAmount;
        private int _lastPreserveAmount;

        public bool IsShowing => gameObject.activeSelf && !MainSettingsMenu.ProfileInput.IsShowing;

        public UnityEvent OnShow;

        public UnityEvent OnHide;

        private IActionUIProfile _profile => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService.GetActiveProfile();

        private IActionUIProfileService _profileService => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService;

        private SelectableTransitions[] _selectables;
        private Selectable _lastSelected;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            if (OnShow == null)
                OnShow = new UnityEvent();
            if (OnHide == null)
                OnHide = new UnityEvent();

            _selectables = GetComponentsInChildren<SelectableTransitions>();

            for (int i = 0; i < _selectables.Length; i++)
            {
                _selectables[i].OnSelected += SettingSelected;
                _selectables[i].OnDeselected += SettingDeselected;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            SetControls();
            HookControls();
        }

        public void Show()
        {
            DebugLogger.Log("StashSettingsView::Show");
            gameObject.SetActive(true);
            SetControls();
            StartCoroutine(SelectNextFrame(MainSettingsMenu.StashViewToggle));

            OnShow?.Invoke();
        }
        public void Hide()
        {
            DebugLogger.Log("StashSettingsView::Hide");
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
            StashInventoryToggle.SetIsOnWithoutNotify(_profile?.StashSettingsProfile?.CharInventoryEnabled ?? false);
            StashInventoryAnywhereToggle.SetIsOnWithoutNotify(_profile?.StashSettingsProfile?.CharInventoryAnywhereEnabled ?? false);

            MerchantStashToggle.SetIsOnWithoutNotify(_profile?.StashSettingsProfile?.MerchantEnabled ?? false);
            MerchantStashAnywhereToggle.SetIsOnWithoutNotify(_profile?.StashSettingsProfile?.MerchantAnywhereEnabled ?? false);

            CraftFromStashToggle.SetIsOnWithoutNotify(_profile?.StashSettingsProfile?.CraftingInventoryEnabled ?? false);
            CraftFromStashAnywhereToggle.SetIsOnWithoutNotify(_profile?.StashSettingsProfile?.CraftingInventoryAnywhereEnabled ?? false);

            PreserveFoodToggle.SetIsOnWithoutNotify(_profile?.StashSettingsProfile?.PreservesFoodEnabled ?? false);
            _lastPreserveAmount = _profile?.StashSettingsProfile?.PreservesFoodAmount ?? 75;
            PreserveFoodAmount.SetTextWithoutNotify(_lastPreserveAmount.ToString());
        }

        private void HookControls()
        {
            StashInventoryToggle.onValueChanged.AddListener((v) => SaveStashSettings());
            StashInventoryAnywhereToggle.onValueChanged.AddListener((v) => SaveStashSettings());
            MerchantStashToggle.onValueChanged.AddListener((v) => SaveStashSettings());
            MerchantStashAnywhereToggle.onValueChanged.AddListener((v) => SaveStashSettings());
            CraftFromStashToggle.onValueChanged.AddListener((v) => SaveStashSettings());
            CraftFromStashAnywhereToggle.onValueChanged.AddListener((v) => SaveStashSettings());
            PreserveFoodToggle.onValueChanged.AddListener((v) => SaveStashSettings());
            //PreserveFoodAmount.onEndEdit.AddListener(ValidateInput);
            PreserveFoodAmount.onValueChanged.AddListener(ValidateInputAndSave);
        }

        private void SaveStashSettings()
        {
            if (_profile.StashSettingsProfile == null)
                _profile.StashSettingsProfile = new StashSettingsProfile();

            _profile.StashSettingsProfile.CharInventoryEnabled = StashInventoryToggle.isOn;
            _profile.StashSettingsProfile.CharInventoryAnywhereEnabled = StashInventoryAnywhereToggle.isOn;
            _profile.StashSettingsProfile.MerchantEnabled = MerchantStashToggle.isOn;
            _profile.StashSettingsProfile.MerchantAnywhereEnabled = MerchantStashAnywhereToggle.isOn;
            _profile.StashSettingsProfile.CraftingInventoryEnabled = CraftFromStashToggle.isOn;
            _profile.StashSettingsProfile.CraftingInventoryAnywhereEnabled = CraftFromStashAnywhereToggle.isOn;
            _profile.StashSettingsProfile.PreservesFoodEnabled = PreserveFoodToggle.isOn;
            int preserveAmount = 75;
            int.TryParse(PreserveFoodAmount.text, out preserveAmount);
            _profile.StashSettingsProfile.PreservesFoodAmount = preserveAmount;
            _profileService.Save();
        }

        private void ValidateInputAndSave(string value)
        {
            int amount;

            if (!int.TryParse(value, out amount))
            {
                PreserveFoodAmount.SetTextWithoutNotify(_lastPreserveAmount.ToString());
                return;
            }

            int maxPreserve = 100;
            int minPreserve = 1;

            if (amount > maxPreserve)
                amount = maxPreserve;
            else if (amount < minPreserve)
                amount = maxPreserve;

            PreserveFoodAmount.text = amount.ToString();
            _lastPreserveAmount = amount;
            if (_profile.StashSettingsProfile.PreservesFoodAmount != amount)
                SaveStashSettings();
        }

        private void SettingSelected(SelectableTransitions transition)
        {
            DebugLogger.Log($"{transition.name} Selected.");
            _lastSelected = transition.Selectable;
        }

        private void SettingDeselected(SelectableTransitions transition)
        {
            DebugLogger.Log($"{transition.name} Deselected.");

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