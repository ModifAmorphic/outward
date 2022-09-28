using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class EquipmentSetsSettingsView : MonoBehaviour, ISettingsView
    {

        public Toggle ArmorSetsCombat;
        public Toggle SkipWeaponAnimation;
        public Toggle EquipFromStash;
        public Toggle StashEquipAnywhere;
        public Toggle UnequipToStash;
        public Toggle StashUnequipAnywhere;

        public MainSettingsMenu MainSettingsMenu;

        public bool IsShowing => gameObject.activeSelf;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        private IActionUIProfileService _profileService => MainSettingsMenu.PlayerMenu.ProfileManager.ProfileService;
        private IActionUIProfile _activeProfile => _profileService.GetActiveProfile();

        private bool _eventsAdded;

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
            DebugLogger.Log("EquipmentSetsSettingsView::Show");
            gameObject.SetActive(true);

            SetControls();
            StartCoroutine(SelectNextFrame(MainSettingsMenu.EquipmentSetViewToggle));

            if (_selectables != null && _selectables.Any())
            {
                _selectables.First().Selectable.Select();
                EventSystem.current.SetSelectedGameObject(_selectables.First().gameObject, MainSettingsMenu.PlayerMenu.PlayerID);
            }
            else
            {
                MainSettingsMenu.EquipmentSetViewToggle.Select();
                EventSystem.current.SetSelectedGameObject(MainSettingsMenu.EquipmentSetViewToggle.gameObject, MainSettingsMenu.PlayerMenu.PlayerID);
            }

            OnShow?.Invoke();
        }

        public void Hide() => Hide(true);

        private void Hide(bool raiseEvent)
        {
            DebugLogger.Log("EquipmentSetsSettingsView::Hide");
            gameObject.SetActive(false);
            if (raiseEvent)
                OnHide?.Invoke();
        }

        private void SetControls()
        {
            ArmorSetsCombat.isOn = _activeProfile.EquipmentSetsSettingsProfile?.ArmorSetsInCombatEnabled ?? false;
            SkipWeaponAnimation.isOn = _activeProfile.EquipmentSetsSettingsProfile?.SkipWeaponAnimationsEnabled ?? false;
            EquipFromStash.isOn = _activeProfile.EquipmentSetsSettingsProfile?.StashEquipEnabled ?? true;
            StashEquipAnywhere.isOn = _activeProfile.EquipmentSetsSettingsProfile?.StashEquipAnywhereEnabled ?? false;
            UnequipToStash.isOn = _activeProfile.EquipmentSetsSettingsProfile?.StashUnequipEnabled ?? true;
            StashUnequipAnywhere.isOn = _activeProfile.EquipmentSetsSettingsProfile?.StashUnequipAnywhereEnabled ?? false;
        }

        private void HookControls()
        {
            ArmorSetsCombat.onValueChanged.AddListener(isOn =>
            {
                GetEquipmentSettingsProfile().ArmorSetsInCombatEnabled = isOn;
                _profileService.Save();
            });

            SkipWeaponAnimation.onValueChanged.AddListener(isOn =>
            {
                GetEquipmentSettingsProfile().SkipWeaponAnimationsEnabled = isOn;
                _profileService.Save();
            });

            EquipFromStash.onValueChanged.AddListener(isOn =>
            {
                GetEquipmentSettingsProfile().StashEquipEnabled = isOn;
                _profileService.Save();
            });

            StashEquipAnywhere.onValueChanged.AddListener(isOn =>
            {
                GetEquipmentSettingsProfile().StashEquipAnywhereEnabled = isOn;
                _profileService.Save();
            });

            UnequipToStash.onValueChanged.AddListener(isOn =>
            {
                GetEquipmentSettingsProfile().StashUnequipEnabled = isOn;
                _profileService.Save();
            });

            StashUnequipAnywhere.onValueChanged.AddListener(isOn =>
            {
                GetEquipmentSettingsProfile().StashUnequipAnywhereEnabled = isOn;
                _profileService.Save();
            });
        }

        private EquipmentSetsSettingsProfile GetEquipmentSettingsProfile()
        {
            if (_activeProfile.EquipmentSetsSettingsProfile == null)
                _activeProfile.EquipmentSetsSettingsProfile = new EquipmentSetsSettingsProfile();

            return _activeProfile.EquipmentSetsSettingsProfile;
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