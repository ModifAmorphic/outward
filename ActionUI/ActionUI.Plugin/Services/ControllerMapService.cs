using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.DataModels;
using ModifAmorphic.Outward.UI.Models;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.UI.Services.Injectors;
using ModifAmorphic.Outward.UI.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Rewired;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ModifAmorphic.Outward.UI.Services
{
    internal class ControllerMapService : IDisposable
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly HotkeyCaptureMenu _captureDialog;
        private readonly ProfileService _profileService;
        private readonly HotbarProfileJsonService _hotbarProfileService;
        private readonly HotbarService _hotbarService;
        private readonly Player _player;
        private readonly ModifCoroutine _coroutine;

        private readonly static HashSet<string> ActionUIMapKeys = new HashSet<string>()
        {
            "RewiredData&playerName=Player0&dataType=ControllerMap&controllerMapType=KeyboardMap&categoryId=131000&layoutId=0&hardwareIdentifier=Keyboard",
            "RewiredData&playerName=Player0&dataType=ControllerMap&controllerMapType=MouseMap&categoryId=131000&layoutId=0&hardwareIdentifier=Mouse",
            "RewiredData&playerName=Player1&dataType=ControllerMap&controllerMapType=KeyboardMap&categoryId=131000&layoutId=0&hardwareIdentifier=Keyboard",
            "RewiredData&playerName=Player1&dataType=ControllerMap&controllerMapType=MouseMap&categoryId=131000&layoutId=0&hardwareIdentifier=Mouse",
        };


        private const string KeyboardMapFile = "KeyboardMap_ActionSlots.xml";
        private const string MouseMapFile = "Mouse_ActionSlots.xml";
        private const string JoystickMapFile = "Joystick_ActionSlots.xml";
        private static Dictionary<ControllerType, string> MapFiles = new Dictionary<ControllerType, string>()
        {
            { ControllerType.Keyboard, KeyboardMapFile },
            { ControllerType.Mouse, MouseMapFile },
            { ControllerType.Joystick, JoystickMapFile }
        };

        public static Dictionary<KeyCode, MouseButton> MouseButtons = new Dictionary<KeyCode, MouseButton>()
        {
            { KeyCode.None, new MouseButton() { KeyCode = KeyCode.None, elementIdentifierId = -1, DisplayName = string.Empty } },
            { KeyCode.Mouse0, new MouseButton() { KeyCode = KeyCode.Mouse0, elementIdentifierId = 3, DisplayName = "LMB" } },
            { KeyCode.Mouse1, new MouseButton() { KeyCode = KeyCode.Mouse2, elementIdentifierId = 4, DisplayName = "RMB" } },
            { KeyCode.Mouse2, new MouseButton() { KeyCode = KeyCode.Mouse2, elementIdentifierId = 5, DisplayName = "MWheel" } },
            { KeyCode.Mouse3, new MouseButton() { KeyCode = KeyCode.Mouse3, elementIdentifierId = 6, DisplayName = "MB 3" } },
            { KeyCode.Mouse4, new MouseButton() { KeyCode = KeyCode.Mouse4, elementIdentifierId = 7, DisplayName = "MB 4" } },
            { KeyCode.Mouse5, new MouseButton() { KeyCode = KeyCode.Mouse5, elementIdentifierId = 8, DisplayName = "MB 5" } },
        };

        public static Dictionary<int, MouseButton> MouseButtonElementIds = new Dictionary<int, MouseButton>()
        {
            { 3, new MouseButton() { KeyCode = KeyCode.Mouse0, elementIdentifierId = 3, DisplayName = "LMB" } },
            { 4, new MouseButton() { KeyCode = KeyCode.Mouse2, elementIdentifierId = 4, DisplayName = "RMB" } },
            { 5, new MouseButton() { KeyCode = KeyCode.Mouse2, elementIdentifierId = 2, DisplayName = "MWheel" } },
            { 6, new MouseButton() { KeyCode = KeyCode.Mouse3, elementIdentifierId = 5, DisplayName = "MB 3" } },
            { 7, new MouseButton() { KeyCode = KeyCode.Mouse4, elementIdentifierId = 6, DisplayName = "MB 4" } },
            { 8, new MouseButton() { KeyCode = KeyCode.Mouse5, elementIdentifierId = 7, DisplayName = "MB 5" } },
        };
        private bool disposedValue;
        private static readonly HashSet<KeyCode> MouseKeyCodes = new HashSet<KeyCode>()
        {
            KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6
        };

        public class MouseButton
        {
            public KeyCode KeyCode { get; set; }
            public int elementIdentifierId { get; set; }
            public string DisplayName { get; set; }
        }


        public ControllerMapService(HotkeyCaptureMenu captureDialog,
                                ProfileManager profileManager,
                                HotbarService hotbarService,
                                Player player,
                                ModifCoroutine coroutine,
                                Func<IModifLogger> getLogger)
        {
            (_captureDialog, _hotbarService, _player, _coroutine, _getLogger) = (captureDialog, hotbarService, player, coroutine, getLogger);

            _profileService = (ProfileService)profileManager.ProfileService;
            _hotbarProfileService = (HotbarProfileJsonService)profileManager.HotbarProfileService;

            RewiredInputsPatches.BeforeExportXmlData += RemoveActionUIMaps;
            RewiredInputsPatches.AfterExportXmlData += RewiredInputsPatches_AfterExportXmlData;
            _profileService.OnActiveProfileSwitched.AddListener(LoadConfigMaps);
            _hotbarProfileService.OnProfileChanged.AddListener(SlotAmountChanged);

            _captureDialog.OnKeysSelected += CaptureDialog_OnKeysSelected;

        }

        private void RewiredInputsPatches_AfterExportXmlData(int playerId)
        {
            if (playerId == _player.id)
            {
                LoadConfigMaps();
            }
        }

        private void RemoveActionUIMaps(int playerId, ref Dictionary<string, string> mappingData)
        {
            if (playerId != _player.id)
                return;

            foreach (string mapKey in ActionUIMapKeys)
            {
                try
                {
                    Logger.LogDebug($"Removing ControllerMap '{mapKey}'");
                    mappingData.Remove(mapKey);
                }
                catch (Exception ex)
                {
                    Logger.LogException($"Exception removing ControllerMap '{mapKey}'", ex);
                }
            }

        }

        private void LoadConfigMaps(IActionUIProfile actionUIProfile) => _ = LoadConfigMaps(true);
        public (KeyboardMap keyboardMap, MouseMap mouseMap) LoadConfigMaps(bool forceRefresh = false)
        {
            var maps = (
                GetActionSlotsMap<KeyboardMap>(forceRefresh),
                GetActionSlotsMap<MouseMap>(forceRefresh)
            );
            if (forceRefresh)
                _hotbarService.ConfigureHotbars(_hotbarProfileService.GetProfile());
            return maps;
        }

        private void CaptureDialog_OnKeysSelected(int slotIndex, HotkeyCategories category, KeyGroup keyGroup)
        {

            (var keyboardMap, var mouseMap) = LoadConfigMaps();

            var maps = new List<ControllerMap>();

            var keyboardKeyName = Keyboard.GetKeyName(keyGroup.KeyCode, GetModifierKeyFlags(keyGroup.Modifiers));

            Logger.LogDebug($"KeyCode {keyGroup.KeyCode} has Keyboard KeyName {keyboardKeyName}.");

            ControllerType controllerType;

            if (!string.IsNullOrWhiteSpace(keyboardKeyName) && !MouseKeyCodes.Contains(keyGroup.KeyCode))
                controllerType = ControllerType.Keyboard;
            else if (MouseKeyCodes.Contains(keyGroup.KeyCode))
            {
                controllerType = ControllerType.Mouse;
            }
            else
                return;

            maps.Add(keyboardMap);
            maps.Add(mouseMap);

            if (category == HotkeyCategories.ActionSlot)
                SetActionSlotHotkey(slotIndex, keyGroup, controllerType, maps);
            else if (category == HotkeyCategories.Hotbar)
                SetHotbarHotkey(slotIndex, keyGroup, controllerType, maps);
            else if (category == HotkeyCategories.NextHotbar || category == HotkeyCategories.PreviousHotbar)
                SetHotbarNavHotkey(category, keyGroup, controllerType, maps);
        }

        private void SlotAmountChanged(IHotbarProfile hotbarProfile, HotbarProfileChangeTypes changeType)
        {
            if (hotbarProfile.Rows == 1)
                return;

            if (changeType == HotbarProfileChangeTypes.SlotAdded)
                ShiftActionSlotsForward(hotbarProfile);
            else if (changeType == HotbarProfileChangeTypes.SlotRemoved)
                ShiftActionSlotsBack(hotbarProfile);
        }

        private void ShiftActionSlotsForward(IHotbarProfile hotbarProfile)
        {
            (var keyboardMap, var mouseMap) = LoadConfigMaps();
            var maps = new List<ControllerMap>() { keyboardMap, mouseMap };

            var firstAddedSlot = hotbarProfile.SlotsPerRow - 1;
            int totalSlots = hotbarProfile.Rows * hotbarProfile.SlotsPerRow;
            var hotbars = hotbarProfile.Hotbars;
            int lastRow = hotbarProfile.Rows - 1;

            for (int r = lastRow; r > 0; r--)
            {
                var firstSlot = r * hotbarProfile.SlotsPerRow;
                var lastSlot = (r + 1) * hotbarProfile.SlotsPerRow - 1;
                for (int s = lastSlot; s >= firstSlot; s--)
                {
                    var slotConfig = hotbars[0].Slots[s].Config as ActionConfig;

                    ControllerType controllerType = ControllerType.Keyboard;
                    // - r because each row adds an new slot and a new offset to account for
                    int previousActionId = ((ActionConfig)hotbars[0].Slots[s].Config).RewiredActionId - r;
                    ActionElementMap previousMap = keyboardMap.ButtonMaps.FirstOrDefault(m => m.actionId == previousActionId);
                    ControllerMap controllerMap = keyboardMap;
                    //if (previousMap == null)
                    //{
                    //    previousMap = mouseMap.ButtonMaps.FirstOrDefault(m => m.actionId == previousSlotConfig.RewiredActionId);
                    //    controllerType = ControllerType.Mouse;
                    //    controllerMap = mouseMap;
                    //}

                    ElementAssignment elementAssignment;

                    if (previousMap == null || s == lastSlot)
                    {
                        elementAssignment = new ElementAssignment(controllerType, ControllerElementType.Button, -1, AxisRange.Positive, KeyCode.None, ModifierKeyFlags.None, slotConfig.RewiredActionId, Pole.Positive, false);
                    }
                    else
                    {
                        elementAssignment = new ElementAssignment(controllerType, ControllerElementType.Button, previousMap.elementIdentifierId, AxisRange.Positive, previousMap.keyCode, previousMap.modifierKeyFlags, slotConfig.RewiredActionId, Pole.Positive, false);
                    }
                    ConfigureButtonMapping(elementAssignment, controllerType, controllerMap, out var hotkeyText);
                    slotConfig.HotkeyText = hotkeyText;
                }
            }
            
            //Clear out the last slot of the first row since it wouldn't be handled above.
            var lastConfig = hotbars[0].Slots[firstAddedSlot].Config as ActionConfig;
            ConfigureButtonMapping(new ElementAssignment(KeyCode.None, ModifierKeyFlags.None, lastConfig.RewiredActionId, Pole.Positive, -1), ControllerType.Keyboard, keyboardMap, out _);
            ConfigureButtonMapping(new ElementAssignment(KeyCode.None, ModifierKeyFlags.None, lastConfig.RewiredActionId, Pole.Positive, -1), ControllerType.Keyboard, mouseMap, out _);
            lastConfig.HotkeyText = string.Empty;

            //set the other bar hotkeys
            for (int b = 1; b < hotbars.Count; b++)
            {
                for (int s = 0; s < hotbars[b].Slots.Count; s++)
                {
                    hotbars[b].Slots[s].Config.HotkeyText = hotbars[0].Slots[s].Config.HotkeyText;
                }
            }

            SaveControllerMap(keyboardMap);
            SaveControllerMap(mouseMap);
        }

        private void ShiftActionSlotsBack(IHotbarProfile hotbarProfile)
        {
            (var keyboardMap, var mouseMap) = LoadConfigMaps();
            var maps = new List<ControllerMap>() { keyboardMap, mouseMap };

            var hotbars = hotbarProfile.Hotbars;

            for (int r = 1; r < hotbarProfile.Rows; r++)
            {
                var firstSlot = r * hotbarProfile.SlotsPerRow;
                var lastSlot = (r + 1) * hotbarProfile.SlotsPerRow - 1;
                for (int s = firstSlot; s <= lastSlot; s++)
                {
                    var slotConfig = hotbars[0].Slots[s].Config as ActionConfig;
                    
                    // + r because each row is offset +1 by the previous rows slot removal
                    int nextActionId = ((ActionConfig)hotbars[0].Slots[s].Config).RewiredActionId + r;

                    ControllerType controllerType = ControllerType.Keyboard;
                    ActionElementMap nextMap = keyboardMap.ButtonMaps.FirstOrDefault(m => m.actionId == nextActionId);
                    ControllerMap controllerMap = keyboardMap;
                    if (nextMap == null)
                    {
                        nextMap = mouseMap.ButtonMaps.FirstOrDefault(m => m.actionId == nextActionId);
                        controllerType = ControllerType.Mouse;
                        controllerMap = mouseMap;
                    }

                    ElementAssignment elementAssignment;
                    if (nextMap == null)
                    {
                        elementAssignment = new ElementAssignment(controllerType, ControllerElementType.Button, -1, AxisRange.Positive, KeyCode.None, ModifierKeyFlags.None, slotConfig.RewiredActionId, Pole.Positive, false);
                    }
                    else
                    {
                        elementAssignment = new ElementAssignment(controllerType, ControllerElementType.Button, nextMap.elementIdentifierId, AxisRange.Positive, nextMap.keyCode, nextMap.modifierKeyFlags, slotConfig.RewiredActionId, Pole.Positive, false);
                    }
                    ConfigureButtonMapping(elementAssignment, controllerType, controllerMap, out var hotkeyText);
                    slotConfig.HotkeyText = hotkeyText;
                }
            }

            //set the other bar hotkeys
            for (int b = 1; b < hotbars.Count; b++)
            {
                for (int s = 0; s < hotbars[b].Slots.Count; s++)
                {
                    hotbars[b].Slots[s].Config.HotkeyText = hotbars[0].Slots[s].Config.HotkeyText;
                }
            }

            SaveControllerMap(keyboardMap);
            SaveControllerMap(mouseMap);
        }

        private void SetActionSlotHotkey(int slotIndex, KeyGroup keyGroup, ControllerType controllerType, List<ControllerMap> maps)
        {

            Logger.LogDebug($"Setting ActionSlot Hotkey for Slot Index {slotIndex} to KeyCode {keyGroup.KeyCode}.");

            var profile = _hotbarProfileService.GetProfile();
            var hotbars = profile.Hotbars;
            var config = (ActionConfig)hotbars[0].Slots[slotIndex].Config;

            ConfigureButtonMapping(config.RewiredActionId, keyGroup, controllerType, maps, out var hotKey);

            for (int b = 0; b < hotbars.Count; b++)
            {
                hotbars[b].Slots[slotIndex].Config.HotkeyText = hotKey;
                Logger.LogDebug($"Setting Hotkey to '{hotKey}' for hotbar {b}, slot {slotIndex}.");
            }

            _hotbarProfileService.Save();
            _hotbarService.ConfigureHotbars(profile);
        }

        private void SetHotbarHotkey(int id, KeyGroup keyGroup, ControllerType controllerType, IEnumerable<ControllerMap> maps)
        {
            Logger.LogDebug($"Setting Hotbar Hotkey for Bar Index {id}.");

            var profile = _hotbarProfileService.GetProfile();
            var hotbar = (HotbarData)profile.Hotbars[id];

            ConfigureButtonMapping(hotbar.RewiredActionId, keyGroup, controllerType, maps, out var hotKey);

            hotbar.HotbarHotkey = hotKey;

            _hotbarProfileService.Save();
            _hotbarService.ConfigureHotbars(profile);
        }

        private void SetHotbarNavHotkey(HotkeyCategories category, KeyGroup keyGroup, ControllerType controllerType, IEnumerable<ControllerMap> maps)
        {
            //Logger.LogDebug($"Setting Hotbar Hotkey for Bar Index {id}.");
            var profile = (HotbarProfileData)_hotbarProfileService.GetProfile();
            var rewiredId = category == HotkeyCategories.NextHotbar ? profile.NextRewiredActionId : profile.PrevRewiredActionId;

            ConfigureButtonMapping(rewiredId, keyGroup, controllerType, maps, out var hotKey);

            if (category == HotkeyCategories.NextHotbar)
            {
                profile.NextHotkey = hotKey;
                Logger.LogDebug($"Setting NextHotkey text to {hotKey}.");
            }
            else
            {
                Logger.LogDebug($"Setting PrevHotkey text to {hotKey}.");
                profile.PrevHotkey = hotKey;
            }

            _hotbarProfileService.Save();
            _hotbarService.ConfigureHotbars(profile);
        }

        private void ConfigureButtonMapping(int rewiredActionId, KeyGroup keyGroup, ControllerType controllerType, IEnumerable<ControllerMap> maps, out string hotkeyText)
        {
            hotkeyText = string.Empty;
            foreach (var map in maps)
            {
                int elementIdentifierId = -1;
                ElementAssignment eleMap;
                if (map.controllerType == controllerType)
                {
                    if (controllerType == ControllerType.Mouse)
                        elementIdentifierId = MouseButtons[keyGroup.KeyCode].elementIdentifierId;

                    Logger.LogDebug($"Configuring Button Mapping for ControllerType {map.controllerType} and actionId {rewiredActionId}. Setting elementIdentifierId to {elementIdentifierId}.  Keycode is {keyGroup.KeyCode}");
                    eleMap = new ElementAssignment(map.controllerType, ControllerElementType.Button, elementIdentifierId, AxisRange.Positive, keyGroup.KeyCode, GetModifierKeyFlags(keyGroup.Modifiers), rewiredActionId, Pole.Positive, false);
                }
                else
                {
                    Logger.LogDebug($"Configuring Removal Button Mapping for ControllerType {map.controllerType} and actionId {rewiredActionId}.");
                    eleMap = new ElementAssignment(map.controllerType, ControllerElementType.Button, elementIdentifierId, AxisRange.Positive, KeyCode.None, ModifierKeyFlags.None, rewiredActionId, Pole.Positive, false);
                }

                ConfigureButtonMapping(eleMap, controllerType, map, out var text);
                if (!string.IsNullOrWhiteSpace(text))
                    hotkeyText = text;

                SaveControllerMap(map);
            }
        }

        private void ConfigureButtonMapping(ElementAssignment assignment, ControllerType controllerType, ControllerMap map, out string hotkeyText)
        {
            hotkeyText = string.Empty;

            var existingMaps = map.ButtonMaps.Where(m => m.actionId == assignment.actionId).ToArray();

            if (existingMaps.Any())
            {
                for (int i = 0; i < existingMaps.Length; i++)
                {
                    if (map.DeleteElementMap(existingMaps[i].id))
                        Logger.LogDebug($"Deleted existing map {existingMaps[i].id} for rewiredId {assignment.actionId}.");
                }
            }

            RemoveExistingAssignments(assignment, map);

            if (map.controllerType == controllerType)
            {
                if (assignment.keyboardKey != KeyCode.None || (map.controllerType == ControllerType.Mouse && MouseButtonElementIds.TryGetValue(assignment.elementIdentifierId, out var mouseEleId) && mouseEleId.KeyCode != KeyCode.None))
                {
                    var result = map.ReplaceOrCreateElementMap(assignment);
                    Logger.LogDebug($"Attempted replace or create element map {assignment.elementMapId} to use keyboardKey {assignment.keyboardKey} in {map.controllerType} ControllerMap {map.id}.  Result was {result}");
                }
                hotkeyText = map.ButtonMaps.FirstOrDefault(m => m.actionId == assignment.actionId)?.elementIdentifierName;
                if (map.controllerType == ControllerType.Mouse && MouseButtonElementIds.TryGetValue(assignment.elementIdentifierId, out var hotkeyEleId))
                    hotkeyText = hotkeyEleId.DisplayName;
            }

        }

        private void RemoveExistingAssignments(ElementAssignment assignment, ControllerMap map)
        {
            var conflict = CreateConflictCheck(map, assignment);
            var removed = map.RemoveElementAssignmentConflicts(conflict);
            Logger.LogDebug($"Removed {removed} conflicts for key {assignment.keyboardKey} and modifiers {assignment.modifierKeyFlags}.");
        }

        private ElementAssignmentConflictCheck CreateConflictCheck(ControllerMap map, ElementAssignment assignment)
        {
            var conflictCheck = assignment.ToElementAssignmentConflictCheck();
            conflictCheck.playerId = map.id;
            conflictCheck.controllerType = map.controllerType;
            conflictCheck.controllerId = map.controllerId;
            conflictCheck.controllerMapId = map.id;
            conflictCheck.controllerMapCategoryId = map.categoryId;
            //if (map.aem != null) conflictCheck.elementMapId = map.aem.id;

            return conflictCheck;
        }

        private void SaveControllerMap(ControllerMap controllerMap)
        {
            var mapXml = controllerMap.ToXmlString();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(mapXml);

            var filePath = Path.Combine(GetProfileFolder(), MapFiles[controllerMap.controllerType]);
            // Save the document to a file and auto-indent the output.
            using (XmlTextWriter writer = new XmlTextWriter(filePath, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                doc.Save(writer);
                writer.Close();
            }


            //File.WriteAllText(filePath, mapXml);
        }

        private T GetActionSlotsMap<T>(bool forceRefresh) where T : ControllerMap
        {
            ControllerType controllerType = ControllerType.Custom;

            if (typeof(KeyboardMap).IsAssignableFrom(typeof(T)))
                controllerType = ControllerType.Keyboard;
            else if (typeof(MouseMap).IsAssignableFrom(typeof(T)))
                controllerType = ControllerType.Mouse;
            else if (typeof(JoystickMap).IsAssignableFrom(typeof(T)))
                controllerType = ControllerType.Joystick;

            Logger.LogDebug($"ActionSlots using {controllerType} ControllerMap for player {_player.id}");

            if (controllerType == ControllerType.Custom)
                return null;

            var map = _player.controllers.maps.GetMap<T>(0, RewiredConstants.ActionSlots.CategoryMapId, 0);
            if (map != null && !forceRefresh)
            {
                Logger.LogDebug($"ActionSlots {controllerType} ControllerMap found loaded for player {_player.id}");
                return map;
            }

            var xml = GetKeyMapFileXml(controllerType);
            var controllerMap = (T)ControllerMap.CreateFromXml(controllerType, xml);
            if (map != null)
            {
                Logger.LogDebug($"Replacing ActionSlots {controllerType} ControllerMap found loaded for player {_player.id}");
                _player.controllers.maps.RemoveMap<T>(0, controllerMap.id);
            }
            _player.controllers.maps.AddMap<T>(0, controllerMap);
            
            return controllerMap;
        }

        private string GetKeyMapFileXml(ControllerType controllerType)
        {
            var hotbarProfile = (HotbarProfileData)_hotbarProfileService.GetProfile();

            string defaultKeyMapFile;

            if (controllerType == ControllerType.Keyboard)
                defaultKeyMapFile = RewiredConstants.ActionSlots.DefaultKeyboardMapFile;
            else if (controllerType == ControllerType.Mouse)
                defaultKeyMapFile = RewiredConstants.ActionSlots.DefaultMouseMapFile;
            else if (controllerType == ControllerType.Joystick)
                defaultKeyMapFile = RewiredConstants.ActionSlots.DefaultJoystickMapFile;
            else
                return String.Empty;

            var keyMapFile = defaultKeyMapFile;

            if (hotbarProfile != null)
            {
                keyMapFile = Path.Combine(GetProfileFolder(), MapFiles[controllerType]);

                if (!File.Exists(keyMapFile))
                    File.Copy(defaultKeyMapFile, keyMapFile);
            }

            Logger.LogDebug($"Loading ActionSlots {controllerType} ControllerMap for player {_player.id} at location '{keyMapFile}'.");
            return File.ReadAllText(keyMapFile);
        }

        private string GetProfileFolder()
        {
            var activeProfile = (ActionUIProfile)_profileService.GetActiveProfile();
            return activeProfile.Path;
        }

        private ModifierKeyFlags GetModifierKeyFlags(IEnumerable<KeyCode> modifiers)
        {
            ModifierKeyFlags modifierKeyFlags = ModifierKeyFlags.None;
            if (modifiers.Contains(KeyCode.RightAlt) || modifiers.Contains(KeyCode.LeftAlt))
                modifierKeyFlags |= ModifierKeyFlags.RightAlt | ModifierKeyFlags.LeftAlt;

            if (modifiers.Contains(KeyCode.RightControl) || modifiers.Contains(KeyCode.LeftControl))
                modifierKeyFlags |= ModifierKeyFlags.RightControl | ModifierKeyFlags.LeftControl;

            if (modifiers.Contains(KeyCode.RightShift) || modifiers.Contains(KeyCode.LeftShift))
                modifierKeyFlags |= ModifierKeyFlags.RightShift | ModifierKeyFlags.LeftShift;


            return modifierKeyFlags;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RewiredInputsPatches.BeforeExportXmlData -= RemoveActionUIMaps;
                    RewiredInputsPatches.AfterExportXmlData -= RewiredInputsPatches_AfterExportXmlData;
                    _profileService.OnActiveProfileSwitched.RemoveListener(LoadConfigMaps);
                    _hotbarProfileService.OnProfileChanged.RemoveListener(SlotAmountChanged);
                    _captureDialog.OnKeysSelected -= CaptureDialog_OnKeysSelected;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ControllerMapService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
