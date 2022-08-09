using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Extensions;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class HotbarService
    {
        private readonly HotbarSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly HotbarsContainer _hotbarsContainer;
        private readonly IHotbarController _hotbars;
        private readonly Player _player;
        private readonly Character _character;
        private readonly CharacterUI _characterUI;
        private readonly IHotbarProfileDataService _profileData;
        private readonly SlotDataService _slotData;

        private readonly LevelCoroutines _levelCoroutines;
        private bool _saveDisabled;
        private bool _isProfileInit;

        private ControllerType _activeController;
        public ControllerType ActiveController => _activeController;

        public HotbarService(HotbarsContainer hotbarsContainer, Player player, Character character, IHotbarProfileDataService profileData, SlotDataService slotData, LevelCoroutines levelCoroutines, HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            if (hotbarsContainer == null)
                throw new ArgumentNullException(nameof(hotbarsContainer));
            if (character == null)
                throw new ArgumentNullException(nameof(character));
            if (profileData == null)
                throw new ArgumentNullException(nameof(profileData));
            if (slotData == null)
                throw new ArgumentNullException(nameof(slotData));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _hotbarsContainer = hotbarsContainer;
            _hotbars = _hotbarsContainer.Controller;

            _player = player;
            _character = character;
            _characterUI = character.CharacterUI;
            _profileData = profileData;
            _slotData = slotData;
            _settings = settings;
            _levelCoroutines = levelCoroutines;
            _getLogger = getLogger;
            
        }

        public void Start()
        {
            _saveDisabled = true;

            QuickSlotPanelPatches.StartInitAfter += DisableKeyboardQuickslots;
            QuickSlotControllerSwitcherPatches.StartInitAfter += SwapCanvasGroup;
            CharacterManagerPatches.AfterApplyQuickSlots += (c) =>
            {
                if (c.UID == _character.UID)
                    QueueActionSlotAssignments();
            };
            var profile = GetOrCreateActiveProfile();
            ConfigureHotbars(profile);
            _hotbarsContainer.ClearChanges();
            _profileData.OnActiveProfileChanged += ConfigureHotbars;
            _levelCoroutines.StartRoutine(CheckProfileForSave());
            _hotbarsContainer.ActionsViewer.ConfigureExit(() => _player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.Cancel)));
        }

        public void ConfigureHotbars(IHotbarProfileData profile)
        {
            _saveDisabled = true;

            SetProfileHotkeys(profile);

            Logger.LogDebug($"Setting Hotbars to {_settings.Hotbars}, Slots per hotbar to {_settings.ActionSlots}");

            _hotbars.ConfigureHotbars(profile);

            if (_isProfileInit)
            {
                AssignSlotActions(profile);
                _saveDisabled = false;
            }
        }

        private IEnumerator CheckProfileForSave()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(5);
                if (_hotbarsContainer.HasChanges && !_saveDisabled)
                {
                    var profile = GetOrCreateActiveProfile();
                    Logger.LogDebug($"Hotbar changes found. Saving active profile '{profile.Name}'");
                    _profileData.SaveProfile(_hotbarsContainer.ToProfileData(profile.Name));
                    _hotbarsContainer.ClearChanges();
                }
            }
        }

        private void SwapCanvasGroup(QuickSlotControllerSwitcher controllerSwitcher, ref CanvasGroup canvasGroup)
        {
            var keyboard = canvasGroup.GetComponent<KeyboardQuickSlotPanel>();
            if (keyboard != null)
            {
                if (keyboard.CharacterUI.RewiredID == _characterUI.RewiredID)
                {
                    _activeController = ControllerType.Keyboard;
                    canvasGroup = _hotbarsContainer.GetComponent<CanvasGroup>();
                    DisableKeyboardQuickslots(keyboard);
                }
            }
            else
                _activeController = ControllerType.Mouse;
        }

        public void DisableKeyboardQuickslots(KeyboardQuickSlotPanel keyboard)
        {
            Logger.LogDebug($"Checking if Keyboard QuickSlots for RewiredID {keyboard.CharacterUI.RewiredID} should be disabled. Comparing to RewiredID {_characterUI.RewiredID}");
            if (keyboard.CharacterUI.RewiredID == _characterUI.RewiredID)
                keyboard.gameObject.SetActive(false);
        }

        private IHotbarProfileData GetOrCreateActiveProfile()
        {
            var activeProfile = _profileData.GetActiveProfile();

            if (activeProfile == null)
            {
                Logger.LogDebug($"No active profile set. Checking if any profiles exist");
                var names = _profileData.GetProfileNames();
                if (names == null || !names.Any())
                {
                    Logger.LogDebug($"No profiles found. Creating default profile '{HotbarSettings.DefaultProfile.Name}'");
                    _profileData.SaveProfile(HotbarSettings.DefaultProfile);
                    names = _profileData.GetProfileNames();
                }
                _profileData.SetActiveProfile(names.First());
                activeProfile = _profileData.GetActiveProfile();
            }
            Logger.LogDebug($"Got or Created Active Profile  '{activeProfile.Name}'");
            return activeProfile;
        }
        

        public void QueueActionSlotAssignments()
        {
            _levelCoroutines.InvokeAfterLevelLoaded(NetworkLevelLoader.Instance, () => AssignSlotActions(GetOrCreateActiveProfile()), 300);
        }
        
        public void AssignSlotActions(IHotbarProfileData profile)
        {
            _saveDisabled = true;
            for (int hb = 0; hb < profile.Hotbars.Count; hb++)
            {
                for (int s = 0; s < profile.Hotbars[hb].Slots.Count; s++)
                {
                    var slot = profile.Hotbars[hb].Slots[s] as SlotData;
                    if (!_slotData.TryGetItemSlotAction(slot, out var slotAction))
                    {
                        _hotbars.GetActionSlots()[hb][s].Controller.AssignEmptyAction();
                    }
                    else
                    {
                        _hotbars.GetActionSlots()[hb][s].Controller.AssignSlotAction(slotAction);
                    }
                }
            }
            SetProfileHotkeys(profile);
            if (!_isProfileInit)
            {
                _isProfileInit = true;
                _hotbarsContainer.ClearChanges();
            }
            _saveDisabled = false;
        }
        private void SetProfileHotkeys(IHotbarProfileData profile)
        {
            var keyMap = _player.controllers.maps.GetMap<KeyboardMap>(0, RewiredConstants.ActionSlots.CategoryMapId, 0);
            foreach (var bar in profile.Hotbars)
            {
                foreach (var slot in bar.Slots)
                {
                    var config = ((ActionConfig)slot.Config);
                        var eleMap = keyMap.ButtonMaps.FirstOrDefault(m => m.actionId == config.RewiredActionId);

                    if (eleMap != null)
                        slot.Config.HotkeyText = eleMap.elementIdentifierName;
                    else
                        slot.Config.HotkeyText = String.Empty;
                }
            }
        }

        private void DoNextFrame(Action action) => _levelCoroutines.StartRoutine(NextFrameCoroutine(action));

        private IEnumerator NextFrameCoroutine(Action action)
        {
            yield return null;
            action.Invoke();
        }

    }
}
