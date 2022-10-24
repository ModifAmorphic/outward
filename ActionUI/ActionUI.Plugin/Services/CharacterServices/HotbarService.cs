using ModifAmorphic.Outward.ActionUI.DataModels;
using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Monobehaviours;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Rewired;
using System;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class HotbarService : IDisposable
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
        private bool _isStarted = false;

        private bool disposedValue;

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

            _player = player;
            _character = character;
            _characterUI = character.CharacterUI;
            _profileManager = profileManager;
            _slotData = slotData;
            _levelCoroutines = levelCoroutines;
            _getLogger = getLogger;

            QuickSlotPanelPatches.StartInitAfter += DisableKeyboardQuickslots;
            SkillMenuPatches.AfterOnSectionSelected += SetSkillsMovable;
            ItemDisplayDropGroundPatches.TryGetIsDropValids.Add(_player.id, TryGetIsDropValid);
            _hotbars.OnAwake += StartNextFrame;
            if (_hotbars.IsAwake)
                StartNextFrame();

        }

        private void StartNextFrame()
        {
            try
            {
                _levelCoroutines.DoNextFrame(() => Start());
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start {nameof(HotbarService)} coroutine {nameof(Start)}.", ex);
            }
        }
        public void Start()
        {
            try
            {
                _isStarted = true;
                _saveDisabled = true;

                SwapCanvasGroup();

                var profile = GetOrCreateActiveProfile();
                TryConfigureHotbars(profile, HotbarProfileChangeTypes.ProfileRefreshed);
                _hotbars.ClearChanges();
                _profileManager.HotbarProfileService.OnProfileChanged += TryConfigureHotbars;
                _hotbars.OnHasChanges.AddListener(Save);

                AssignSlotActions();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start {nameof(HotbarService)}.", ex);
            }
        }

        private void SetSkillsMovable(ItemListDisplay itemListDisplay)
        {
            if (!_profileManager?.ProfileService?.GetActiveProfile()?.ActionSlotsEnabled ?? false)
                return;

            var displays = itemListDisplay.GetComponentsInChildren<ItemDisplay>();
            for (int i = 0; i < displays.Length; i++)
            {
                displays[i].Movable = true;
            }
        }

        private bool TryGetIsDropValid(ItemDisplay draggedDisplay, Character character, out bool result)
        {
            result = false;
            if (draggedDisplay?.RefItem == null || !(draggedDisplay.RefItem is Skill skill))
                return false;

            Logger.LogDebug($"TryGetIsDropValid:: Blocking drop of skill {skill.name} to DropPanel.");
            return true;
        }

        public void TryConfigureHotbars(IHotbarProfile profile)
        {
            try
            {
                if (!_hotbars.IsAwake || !_isStarted || _character?.Inventory == null)
                    return;

                _saveDisabled = true;

                SetProfileHotkeys(profile);

                _hotbars.Controller.ConfigureHotbars(profile);

                if (_isProfileInit)
                {
                    AssignSlotActions(profile);
                    _saveDisabled = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to configure Hotbars.", ex);
            }
        }
        private void TryConfigureHotbars(IHotbarProfile profile, HotbarProfileChangeTypes changeType)
        {
            try
            {
                TryConfigureHotbars(profile);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to configure Hotbars.", ex);
            }
        }

        public Guid InstanceID { get; } = Guid.NewGuid();
        private void Save()
        {
            try
            {
                if (_hotbars.HasChanges && !_saveDisabled)
                {
                    var profile = GetOrCreateActiveProfile();
                    Logger.LogDebug($"{nameof(HotbarService)}_{InstanceID}: Hotbar changes detected. Saving.");
                    _profileManager.HotbarProfileService.Update(_hotbars);
                    _hotbars.ClearChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to Save Hotbar changes.", ex);
            }
        }

        private void SwapCanvasGroup()
        {
            try
            {
                var controllerSwitcher = _characterUI.GetComponentsInChildren<QuickSlotControllerSwitcher>().FirstOrDefault(q => q.name == "QuickSlot");
                var canvasGroup = controllerSwitcher.GetPrivateField<QuickSlotControllerSwitcher, CanvasGroup>("m_keyboardQuickSlots");

                var hotbarCanvasGroup = _hotbars.GetComponent<CanvasGroup>();
                controllerSwitcher.SetPrivateField("m_keyboardQuickSlots", hotbarCanvasGroup);

                var keyboard = controllerSwitcher.GetComponentInChildren<KeyboardQuickSlotPanel>(true);
                if (keyboard != null)
                {
                    DisableKeyboardQuickslots(keyboard);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Failed to swap in Hotbar canvas group. Action Slots will likely not automaticaly hide when a controller is used.", ex);
            }

        }

        public void DisableKeyboardQuickslots(KeyboardQuickSlotPanel keyboard)
        {
            if (keyboard != null && keyboard.gameObject.activeSelf)
            {
                Logger.LogDebug($"Disabling Keyboard QuickSlots for RewiredID {keyboard?.CharacterUI?.RewiredID}.");
                keyboard.gameObject.SetActive(false);
            }
        }


        private IHotbarProfile GetOrCreateActiveProfile()
        {
            var activeProfile = _profileManager.ProfileService.GetActiveProfile();

            var hotbarProfile = _profileManager.HotbarProfileService.GetProfile();
            if (hotbarProfile == null)
                _profileManager.HotbarProfileService.SaveNew(HotbarSettings.DefaulHotbarProfile);

            Logger.LogDebug($"Got or Created Active Profile '{activeProfile.Name}'");
            return _profileManager.HotbarProfileService.GetProfile();
        }

        public void AssignSlotActions() => AssignSlotActions(GetOrCreateActiveProfile());

        public void AssignSlotActions(IHotbarProfile profile)
        {
            Logger.LogDebug($"{nameof(HotbarService)}_{InstanceID}: Assigning Slot Actions.");
            
            //_characterUI.ShowMenu(CharacterUI.MenuScreens.Inventory);
            //_characterUI.HideMenu(CharacterUI.MenuScreens.Inventory);

            _saveDisabled = true;
            for (int hb = 0; hb < profile.Hotbars.Count; hb++)
            {
                for (int s = 0; s < profile.Hotbars[hb].Slots.Count; s++)
                {
                    try
                    {
                        var slotData = profile.Hotbars[hb].Slots[s] as SlotData;
                        var actionSlot = _hotbars.Controller.GetActionSlots()[hb][s];
                        if (!_slotData.TryGetItemSlotAction(slotData, profile.CombatMode, out var slotAction))
                        {
                            actionSlot.Controller.AssignEmptyAction();
                        }
                        else
                        {
                            try
                            {
                                actionSlot.Controller.AssignSlotAction(slotAction);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogException($"Failed to assign slot action '{slotAction.DisplayName}' to Bar {hb}, Slot Index {s}.", ex);
                            }
                        }
                        actionSlot.ActionButton.gameObject.GetOrAddComponent<ActionSlotDropper>().SetLogger(_getLogger);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException($"Failed to assign action to slot {hb}_{s}.", ex);
                    }
                }
            }
            SetProfileHotkeys(profile);
            if (!_isProfileInit)
            {
                _isProfileInit = true;
            }
            Logger.LogDebug($"Clearing Hotbar Change Flag.");
            _hotbars.ClearChanges();
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
                        slot.Config.HotkeyText = string.Empty;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Logger.LogDebug($"Disposing of {nameof(HotbarService)} instance '{InstanceID}'. Unsubscribing to events.");

                    QuickSlotPanelPatches.StartInitAfter -= DisableKeyboardQuickslots;
                    SkillMenuPatches.AfterOnSectionSelected -= SetSkillsMovable;

                    if (ItemDisplayDropGroundPatches.TryGetIsDropValids.ContainsKey(_player.id))
                        ItemDisplayDropGroundPatches.TryGetIsDropValids.Remove(_player.id);

                    if (_hotbars != null)
                    {
                        _hotbars.OnHasChanges.RemoveListener(Save);
                        _hotbars.OnAwake -= StartNextFrame;
                    }
                    if (_profileManager?.HotbarProfileService != null)
                        _profileManager.HotbarProfileService.OnProfileChanged -= TryConfigureHotbars;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
