using ModifAmorphic.Outward.UI.DataModels;
using ModifAmorphic.Outward.UI.Extensions;
using ModifAmorphic.Outward.UI.Models;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.UI.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
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
using UnityEngine.UI;

namespace ModifAmorphic.Outward.UI.Services
{
    internal class HotbarServiceSwitched
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

        private QuickSlotControllerSwitcher _quickSlotControllerSwitcher;
        private KeyboardQuickSlotPanel _quickslotKeyboard;
        private CanvasGroup _quickslotKeyboardCanvasGroup;
        private bool _hotbarsEnabled;

        public HotbarServiceSwitched(HotbarsContainer hotbarsContainer, Player player, Character character, ProfileManager profileManager, SlotDataService slotData, LevelCoroutines levelCoroutines, Func<IModifLogger> getLogger)
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

            QuickSlotPanelPatches.StartInitAfter += (keyboard) =>
            {
                if (_quickslotKeyboard == null)
                    _quickslotKeyboard = keyboard;
                if (_profileManager.ProfileService.GetActiveProfile().ActionSlotsEnabled)
                {
                    DisableKeyboardQuickslots();
                }
            };
            QuickSlotControllerSwitcherPatches.StartInitAfter += SwapCanvasGroup;

            NetworkLevelLoader.Instance.onOverallLoadingDone += () =>
            {
                if (_profileManager.ProfileService.GetActiveProfile().ActionSlotsEnabled)
                    AssignSlotActions(GetOrCreateActiveProfile());
                _isProfileInit = true;
            };

            _profileManager.ProfileService.OnActiveProfileChanged.AddListener(profile =>
            {
                if (profile.ActionSlotsEnabled && !_hotbarsEnabled)
                    EnableHotbars();
                else if (!profile.ActionSlotsEnabled && _hotbarsEnabled)
                    EnableKeyboardQuickslots();
            });
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
            if (_profileManager.ProfileService.GetActiveProfile().ActionSlotsEnabled)
            {
                ConfigureHotbars(profile);
                _hotbars.ClearChanges();
                _profileManager.HotbarProfileService.OnProfileChanged.AddListener(ConfigureHotbars);
                _levelCoroutines.StartRoutine(CheckProfileForSave());
            }
            else
            {
                ToggleQuickslotPositonable(true);
            }
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

        private void SwapCanvasGroup(QuickSlotControllerSwitcher controllerSwitcher)
        {
            if (controllerSwitcher.name != "QuickSlot")
                return;
            _quickSlotControllerSwitcher = controllerSwitcher;
            var canvasGroup = _quickSlotControllerSwitcher.GetPrivateField<QuickSlotControllerSwitcher, CanvasGroup>("m_keyboardQuickSlots");
            var keyboard = canvasGroup.GetComponent<KeyboardQuickSlotPanel>();
            if (keyboard != null)
            {
                if (keyboard.CharacterUI.RewiredID == _characterUI.RewiredID)
                {
                    if (_quickslotKeyboardCanvasGroup == null)
                        _quickslotKeyboardCanvasGroup = canvasGroup;
                    if (_quickslotKeyboard == null)
                        _quickslotKeyboard = keyboard;
                    _activeController = ControllerType.Keyboard;
                    if (_profileManager.ProfileService.GetActiveProfile().ActionSlotsEnabled)
                        EnableHotbars();
                    else
                        DisableHotbars();
                }
            }
            else
                _activeController = ControllerType.Mouse;
        }

        public void EnableKeyboardQuickslots()
        {
            Logger.LogDebug($"Enabling Keyboard Quickslots for player RewiredID {_quickslotKeyboard?.CharacterUI?.RewiredID}.");
            if (_quickslotKeyboard != null && _quickslotKeyboardCanvasGroup != null)
            {
                _quickslotKeyboard.gameObject.SetActive(true);
                _quickSlotControllerSwitcher.SetPrivateField("m_keyboardQuickSlots", _quickslotKeyboardCanvasGroup);
                ControlsInput.SetQuickSlotActive(_characterUI.RewiredID, true);
                DisableHotbars();
                ToggleQuickslotPositonable(true);
            }
        }

        public void DisableKeyboardQuickslots()
        {
            if (_quickslotKeyboard != null && _quickslotKeyboard.CharacterUI.RewiredID == _characterUI.RewiredID)
            {
                Logger.LogDebug($"Disabling Keyboard QuickSlots for RewiredID {_quickslotKeyboard?.CharacterUI?.RewiredID}.");
                ToggleQuickslotPositonable(false);
                _quickslotKeyboard.gameObject.SetActive(false);
            }
        }

        public void EnableHotbars()
        {
            if (_quickSlotControllerSwitcher != null)
            {
                _hotbars.Controller.EnableHotbars();
                _quickSlotControllerSwitcher.SetPrivateField("m_keyboardQuickSlots", _hotbars.GetComponent<CanvasGroup>());
                DisableKeyboardQuickslots();
                ControlsInput.SetQuickSlotActive(_characterUI.RewiredID, false);

                ConfigureHotbars(GetOrCreateActiveProfile());
                _hotbars.ClearChanges();
                _profileManager.HotbarProfileService.OnProfileChanged.AddListener(ConfigureHotbars);
                
                var positionable = _hotbars.GetComponent<PositionableUI>();
                positionable.SetPositionFromProfile(_profileManager.PositionsProfileService.GetProfile());
                if (positionable.BackgroundImage != null && positionable.BackgroundImage.gameObject.activeSelf)
                    positionable.BackgroundImage.gameObject.SetActive(false);

                _levelCoroutines.StartRoutine(CheckProfileForSave());
                _hotbarsEnabled = true;
            }
        }

        public void DisableHotbars()
        {
            _profileManager.HotbarProfileService.OnProfileChanged.RemoveListener(ConfigureHotbars);
            _levelCoroutines.StopRoutine(CheckProfileForSave());

            _hotbars.Controller.DisableHotbars();
            _hotbarsEnabled = false;
        }

        private void ToggleQuickslotPositonable(bool enabled)
        {
            
            var keyboard = _characterUI.transform.Find("Canvas/GameplayPanels/HUD/QuickSlot/Keyboard");
            var positionable = keyboard.GetComponent<PositionableUI>();
            if (enabled)
            {
                var slotsGroup = keyboard.GetComponent<HorizontalLayoutGroup>();
                var slots = slotsGroup.GetComponentsInChildren<EditorQuickSlotDisplayPlacer>();
                var slotRect = slots.First().GetComponent<RectTransform>().rect;
                var width = (slotRect.width + slotsGroup.spacing) * 8 + slotsGroup.padding.horizontal * 2 - slotsGroup.spacing;
                var height = slotRect.height + slotsGroup.padding.horizontal * 2;
                Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(ToggleQuickslotPositonable)}(): {slots.Length} QuickSlots found. Setting PositionableBg rect dimensions to ({width}, {height}).  Slot size ({slotRect.width}, {slotRect.height})");
                var rectTranform = positionable.BackgroundImage.GetComponent<RectTransform>();
                rectTranform.anchorMin = new Vector2(1f, 0f);
                rectTranform.anchorMax = new Vector2(1f, 1f);
                rectTranform.pivot = new Vector2(1f, .5f);
                rectTranform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * 1.2f);

                positionable.SetPositionFromProfile(_profileManager.PositionsProfileService.GetProfile());
            }

            positionable.enabled = enabled;
        }

        private IHotbarProfile GetOrCreateActiveProfile()
        {
            var activeProfile = _profileManager.ProfileService.GetActiveProfile();

            if (activeProfile == null)
            {
                Logger.LogDebug($"No active profile set. Checking if any profiles exist");
                var names = _profileManager.ProfileService.GetProfileNames();
                if (names == null || !names.Any())
                {
                    Logger.LogDebug($"No profiles found. Creating default profile '{ActionUISettings.DefaultProfile.Name}'");
                    _profileManager.ProfileService.SaveNew(ActionUISettings.DefaultProfile);
                    names = _profileManager.ProfileService.GetProfileNames();
                }
                else
                    _profileManager.ProfileService.SetActiveProfile(names.First());
            }

            var hotbarProfile = _profileManager.HotbarProfileService.GetProfile();
            if (hotbarProfile == null)
                _profileManager.HotbarProfileService.SaveNew(HotbarSettings.DefaulHotbarProfile);

            Logger.LogDebug($"Got or Created Active Profile '{activeProfile.Name}'");
            return _profileManager.HotbarProfileService.GetProfile();
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
            var mouseMap = _player.controllers.maps.GetMap<MouseMap>(0, RewiredConstants.ActionSlots.CategoryMapId, 0);
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

            foreach (HotbarData bar in profileData.Hotbars)
            {
                var buttonMap = mouseMap.ButtonMaps.FirstOrDefault(m => m.actionId == bar.RewiredActionId);
                if (buttonMap != null)
                    bar.HotbarHotkey = ControllerMapService.MouseButtonElementIds[buttonMap.elementIdentifierId].DisplayName;
                foreach (var slot in bar.Slots)
                {
                    var config = ((ActionConfig)slot.Config);
                    var eleMap = mouseMap.ButtonMaps.FirstOrDefault(m => m.actionId == config.RewiredActionId);

                    if (eleMap != null)
                    {
                        slot.Config.HotkeyText = ControllerMapService.MouseButtonElementIds[eleMap.elementIdentifierId].DisplayName;
                    }
                }
            }
        }
    }
}
