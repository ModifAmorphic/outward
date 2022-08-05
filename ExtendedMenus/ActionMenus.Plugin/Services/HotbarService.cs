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

        private IActionSlotConfig[,] _actionSlotConfigs;

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

            //_hotbarsContainer.OnChanged += (hbc) => {
            //    var name = GetOrCreateActiveProfile().Name;
            //    _profileData.SaveProfile(hbc.ToHotbarProfileData(name));
            //};
            //settings.HotbarsChanged += (bars) => ConfigureSlots();
            //settings.ActionSlotsChanged += (slots) =>
            //{
            //    ConfigureSlots();
            //    AssignSlotActions(_character);
            //};

            _saveDisabled = true;
            _isProfileInit = true;
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


        //public void ConfigureSlots()
        //{
        //    var profile = GetOrCreateActiveProfile();
        //    ConfigureHotbars(profile);

        //    //_actionSlotConfigs = new ActionSlotConfig[_settings.Hotbars, _settings.ActionSlots];
        //    //for (int b = 0; b < _settings.Hotbars; b++)
        //    //{
        //    //    for (int s = 0; s < _settings.ActionSlots; s++)
        //    //    {
        //    //        _actionSlotConfigs[b, s] = (new ActionSlotConfig()
        //    //        {
        //    //            ShowZeroStackAmount = false,
        //    //            ShowCooldownTime = false,
        //    //            EmptySlotOption = EmptySlotOptions.Image,
        //    //            HotkeyText = (s + 1).ToString(),
        //    //        });
        //    //    }
        //    //}

        //    ////_hotbars.ConfigureHotbars(profile.HotbarAssignments.Count, profile.Rows, profile.SlotsPerRow, _actionSlotConfigs, keyListener);



        //    //DoNextFrame(() =>
        //    //{
        //    //    for (int b = 0; b < _settings.Hotbars; b++)
        //    //    {
        //    //        for (int s = 0; s < _settings.ActionSlots; s++)
        //    //        {
        //    //            _hotbars.GetActionSlots()[b][s].Controller.Configure(new ActionSlotConfig()
        //    //            {
        //    //                ShowZeroStackAmount = false,
        //    //                ShowCooldownTime = false,
        //    //                EmptySlotOption = EmptySlotOptions.Image,
        //    //                HotkeyText = (s + 1).ToString(),
        //    //            });
        //    //        }
        //    //    }
        //    //});
        //}

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
        private void ConfigureHotbars(IHotbarProfileData profile)
        {
            _saveDisabled = true;
            
            SetProfileHotkeys(profile);

            Logger.LogDebug($"Setting Hotbars to {_settings.Hotbars}, Slots per hotbar to {_settings.ActionSlots}");

            var keyListener = new HotbarKeyListener(_player);
            _hotbars.ConfigureHotbars(profile, keyListener);
            if (!_isProfileInit)
            {
                AssignSlotActions(profile);
                //_saveDisabled = false;
            }
            
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
            if (_isProfileInit)
            {
                _isProfileInit = false;
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
        //public void AssignSlotActions(Character character)
        //{
        //    if (character.UID != _character.UID)
        //        return;

        //    //_hotbarsContainer.Controller.RegisterActionViewData(new SlotActionViewData(_player, _character, _getLogger));
        //    Psp.GetServicesProvider(_player.id).AddSingleton<IActionViewData>(new SlotActionViewData(_player, _character, _getLogger));

        //    _hotbarsContainer.ActionsViewer.ConfigureExit(() => _player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.Cancel)));

        //    string nameFormat = "ActionSlot_00";
        //    int slotIndex = 0;
        //    var learnedItems = character.Inventory.SkillKnowledge.GetLearnedItems();
        //    Logger.LogDebug($"Character has {learnedItems.Count} learned skills.");
        //    foreach (var item in learnedItems)
        //    {
        //        var actionSlot = _hotbars.GetActionSlots()[0][slotIndex];
        //        var actionName = (slotIndex + 1).ToString(nameFormat);
        //        TryAssignAction(actionSlot, actionName, item);
        //        slotIndex++;
        //        if (slotIndex >= _hotbars.GetActionSlots()[0].Length)
        //            break;
        //    }
        //}

        //public bool TryAssignAction(ActionSlot actionSlot, string actionName, Item item)
        //{
        //    try
        //    {
        //        Logger.LogDebug($"Assigning item {item?.name} to action slot {actionSlot?.name} and Rewired Action {actionName}.");
        //        actionSlot.Controller.AssignSlotAction(GetSlotAction(item));
        //        //This needs to be set to pass certain checks in the base game.
        //        //item.SetQuickSlot(actionSlot.SlotIndex);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"Encountered exception assigning action {actionName} for Item/Skill {item?.name}.", ex);
        //        return false;
        //    }
        //}
        //public void ToggleEditMode(bool enabled)
        //{
        //    _hotbars.ToggleEditMode(enabled);
        //}
        //private ISlotAction GetSlotAction(Item item)
        //{
        //    var slotAction = new ItemSlotAction(item, _player, _characterUI.TargetCharacter, _getLogger)
        //    {
        //        Cooldown = new ItemCooldown(item),
        //        Stack = item.IsStackable() ? item.ToStackable(_character.Inventory) : null,
        //        //TargetAction = () => Logger.LogInfo($"Action {actionName} triggered!")
        //    };
        //    return slotAction;

        //}

        private void DoNextFrame(Action action) => _levelCoroutines.StartRoutine(NextFrameCoroutine(action));

        private IEnumerator NextFrameCoroutine(Action action)
        {
            yield return null;
            action.Invoke();
        }

    }
}
