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

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly HotbarsContainer _hotbars;
        private readonly Player _player;
        private readonly Character _character;
        private readonly CharacterUI _characterUI;
        private readonly ProfileManager _profileManager;
        private readonly SlotDataService _slotData;

        private readonly LevelCoroutines _levelCoroutines;
        private bool _saveDisabled;
        private bool _isProfileInit;

        private ControllerType _activeController;
        public ControllerType ActiveController => _activeController;

        public HotbarService(HotbarsContainer hotbarsContainer, Player player, Character character, ProfileManager profileManager, SlotDataService slotData, LevelCoroutines levelCoroutines, Func<IModifLogger> getLogger)
        {
            if (hotbarsContainer == null)
                throw new ArgumentNullException(nameof(hotbarsContainer));
            if (character == null)
                throw new ArgumentNullException(nameof(character));
            if (profileManager == null)
                throw new ArgumentNullException(nameof(profileManager));
            if (slotData == null)
                throw new ArgumentNullException(nameof(slotData));

            _hotbars = hotbarsContainer;
            //_hotbars = _hotbarsContainer.Controller;

            _player = player;
            _character = character;
            _characterUI = character.CharacterUI;
            _profileManager = profileManager;
            _slotData = slotData;
            _levelCoroutines = levelCoroutines;
            _getLogger = getLogger;

            QuickSlotPanelPatches.StartInitAfter += DisableKeyboardQuickslots;
            QuickSlotControllerSwitcherPatches.StartInitAfter += SwapCanvasGroup;

            NetworkLevelLoader.Instance.onOverallLoadingDone += () =>
            {
                AssignSlotActions(GetOrCreateActiveProfile());
            };
            //CharacterManagerPatches.AfterApplyQuickSlots += (c) =>
            //{
            //    if (c.UID == _character.UID)
            //        QueueActionSlotAssignments();
            //};
        }

        public void Start()
        {
            _saveDisabled = true;

            var profile = GetOrCreateActiveProfile();
            ConfigureHotbars(profile);
            _hotbars.ClearChanges();
            _profileManager.HotbarProfileService.OnProfileChanged.AddListener(ConfigureHotbars);
            _levelCoroutines.StartRoutine(CheckProfileForSave());
        }

        public void ConfigureHotbars(IHotbarProfile profile)
        {
            _saveDisabled = true;

            SetProfileHotkeys(profile);

            _hotbars.Controller.ConfigureHotbars(profile);

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
                if (_hotbars.HasChanges && !_saveDisabled)
                {
                    var profile = GetOrCreateActiveProfile();
                    Logger.LogDebug($"Hotbar changes found. Saving.");
                    _profileManager.HotbarProfileService.Update(_hotbars);
                    _hotbars.ClearChanges();
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
                    canvasGroup = _hotbars.GetComponent<CanvasGroup>();
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

        private IHotbarProfile GetOrCreateActiveProfile()
        {
            var activeProfile = _profileManager.GetActiveProfile();

            if (activeProfile == null)
            {
                Logger.LogDebug($"No active profile set. Checking if any profiles exist");
                var names = _profileManager.GetProfileNames();
                if (names == null || !names.Any())
                {
                    Logger.LogDebug($"No profiles found. Creating default profile '{ActionMenuSettings.DefaultProfile.Name}'");
                    _profileManager.ProfileService.SaveNew(ActionMenuSettings.DefaultProfile);
                    names = _profileManager.GetProfileNames();
                }
                else
                    _profileManager.SetActiveProfile(names.First());
            }

            var hotbarProfile = _profileManager.HotbarProfileService.GetProfile();
            if (hotbarProfile == null)
                _profileManager.HotbarProfileService.SaveNew(HotbarSettings.DefaulHotbarProfile);

            Logger.LogDebug($"Got or Created Active Profile  '{activeProfile.Name}'");
            return _profileManager.HotbarProfileService.GetProfile();
        }
        

        public void QueueActionSlotAssignments()
        {
            //var invokeTime = DateTime.Now.AddSeconds(10);
            //_levelCoroutines.StartRoutine(
            //    _levelCoroutines.InvokeAfter(() => DateTime.Now >= invokeTime, () => AssignSlotActions(GetOrCreateActiveProfile()), 30)
            //);
            //_levelCoroutines.InvokeAfterLevelAndPlayersLoaded(NetworkLevelLoader.Instance, () => AssignSlotActions(GetOrCreateActiveProfile()), 300);
        }
        
        public void AssignSlotActions(IHotbarProfile profile)
        {
            //refresh item displays
            _characterUI.ShowMenu(CharacterUI.MenuScreens.Inventory);
            _characterUI.HideMenu(CharacterUI.MenuScreens.Inventory);

            _saveDisabled = true;
            //_characterUI.InventoryPanel.RefreshEquippedBag
            for (int hb = 0; hb < profile.Hotbars.Count; hb++)
            {
                for (int s = 0; s < profile.Hotbars[hb].Slots.Count; s++)
                {
                    var slot = profile.Hotbars[hb].Slots[s] as SlotData;
                    if (!_slotData.TryGetItemSlotAction(slot, profile.CombatMode, out var slotAction))
                    {
                        _hotbars.Controller.GetActionSlots()[hb][s].Controller.AssignEmptyAction();
                    }
                    else
                    {
                        _hotbars.Controller.GetActionSlots()[hb][s].Controller.AssignSlotAction(slotAction);
                    }
                }
            }
            SetProfileHotkeys(profile);
            if (!_isProfileInit)
            {
                _isProfileInit = true;
                _hotbars.ClearChanges();
            }
            _saveDisabled = false;
        }
        private void SetProfileHotkeys(IHotbarProfile profile)
        {
            var keyMap = _player.controllers.maps.GetMap<KeyboardMap>(0, RewiredConstants.ActionSlots.CategoryMapId, 0);
            var profileData = (HotbarProfileData)profile;
            profileData.NextHotkey = keyMap.ButtonMaps.FirstOrDefault(m => m.actionId == profileData.NextRewiredActionId)?.elementIdentifierName;
            profileData.PrevHotkey = keyMap.ButtonMaps.FirstOrDefault(m => m.actionId == profileData.PrevRewiredActionId)?.elementIdentifierName;

            foreach (HotbarData bar in profileData.Hotbars)
            {
                bar.HotbarHotkey = keyMap.ButtonMaps.FirstOrDefault(m => m.actionId == bar.RewiredActionId)?.elementIdentifierName;
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
    }
}
