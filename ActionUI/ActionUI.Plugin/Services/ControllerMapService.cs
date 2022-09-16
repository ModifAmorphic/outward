using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.DataModels;
using ModifAmorphic.Outward.UI.Models;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.UI.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Rewired;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ModifAmorphic.Outward.UI.Services
{
    internal class ControllerMapService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly HotkeyCaptureMenu _captureDialog;
        private readonly ProfileService _profileService;
        private readonly HotbarProfileJsonService _hotbarData;
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
            _hotbarData = (HotbarProfileJsonService)profileManager.HotbarProfileService;

            RewiredInputsPatches.BeforeExportXmlData += RemoveActionUIMaps;
            RewiredInputsPatches.AfterExportXmlData += RewiredInputsPatches_AfterExportXmlData;

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

        public void LoadConfigMaps()
        {
            GetActionSlotsMap<KeyboardMap>();
            GetActionSlotsMap<MouseMap>();
        }

        private void CaptureDialog_OnKeysSelected(int id, HotkeyCategories category, KeyGroup keyGroup)
        {

            var keyboardMap = GetActionSlotsMap<KeyboardMap>();
            var mouseMap = GetActionSlotsMap<MouseMap>();

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
                SetActionSlotHotkey(id, keyGroup, controllerType, maps);
            else if (category == HotkeyCategories.Hotbar)
                SetHotbarHotkey(id, keyGroup, controllerType, maps);
            else if (category == HotkeyCategories.NextHotbar || category == HotkeyCategories.PreviousHotbar)
                SetHotbarNavHotkey(category, keyGroup, controllerType, maps);
        }

        private void SetActionSlotHotkey(int id, KeyGroup keyGroup, ControllerType controllerType, List<ControllerMap> maps)
        {

            Logger.LogDebug($"Setting ActionSlot Hotkey for Slot Index {id} to KeyCode {keyGroup.KeyCode}.");

            var profile = _hotbarData.GetProfile();
            var hotbars = profile.Hotbars;
            var config = (ActionConfig)hotbars[0].Slots[id].Config;

            ConfigureButtonMapping(config.RewiredActionId, keyGroup, controllerType, maps, out var hotKey);

            for (int b = 0; b < hotbars.Count; b++)
            {
                hotbars[b].Slots[id].Config.HotkeyText = hotKey;
                Logger.LogDebug($"Setting Hotkey to '{hotKey}' for hotbar {b}, slot {id}.");
            }

            _hotbarData.Save();
            _hotbarService.ConfigureHotbars(profile);
        }

        private void SetHotbarHotkey(int id, KeyGroup keyGroup, ControllerType controllerType, IEnumerable<ControllerMap> maps)
        {
            Logger.LogDebug($"Setting Hotbar Hotkey for Bar Index {id}.");

            var profile = _hotbarData.GetProfile();
            var hotbar = (HotbarData)profile.Hotbars[id];

            ConfigureButtonMapping(hotbar.RewiredActionId, keyGroup, controllerType, maps, out var hotKey);

            hotbar.HotbarHotkey = hotKey;

            _hotbarData.Save();
            _hotbarService.ConfigureHotbars(profile);
        }

        private void SetHotbarNavHotkey(HotkeyCategories category, KeyGroup keyGroup, ControllerType controllerType, IEnumerable<ControllerMap> maps)
        {
            //Logger.LogDebug($"Setting Hotbar Hotkey for Bar Index {id}.");
            var profile = (HotbarProfileData)_hotbarData.GetProfile();
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

            _hotbarData.Save();
            _hotbarService.ConfigureHotbars(profile);
            //SaveControllerMap(map);
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
                var existingMaps = map.ButtonMaps.Where(m => m.actionId == rewiredActionId).ToArray();

                if (existingMaps.Any())
                {
                    for (int i = 0; i < existingMaps.Length; i++)
                    {
                        if (map.DeleteElementMap(existingMaps[i].id))
                            Logger.LogDebug($"Deleted existing map {existingMaps[i].id} for rewiredId {rewiredActionId}.");
                    }
                }

                RemoveExistingAssignments(eleMap, map);

                if (map.controllerType == controllerType)
                {
                    if (keyGroup.KeyCode != KeyCode.None)
                    {
                        var result = map.ReplaceOrCreateElementMap(eleMap);
                        Logger.LogDebug($"Attempted replace or create element map {eleMap.elementMapId} to use KeyCode {keyGroup.KeyCode} in {map.controllerType} ControllerMap {map.id}.  Result was {result}");
                    }
                    hotkeyText = map.ButtonMaps.FirstOrDefault(m => m.actionId == rewiredActionId)?.elementIdentifierName;
                    if (map.controllerType == ControllerType.Mouse)
                        hotkeyText = MouseButtons[keyGroup.KeyCode].DisplayName;
                }

                SaveControllerMap(map);
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

        private T GetActionSlotsMap<T>() where T : ControllerMap
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
            if (map != null)
            {
                Logger.LogDebug($"ActionSlots {controllerType} ControllerMap found for player {_player.id}");
                return map;
            }

            var xml = GetKeyMapFileXml(controllerType);
            var controllerMap = (T)ControllerMap.CreateFromXml(controllerType, xml);
            _player.controllers.maps.AddMap<T>(0, controllerMap);
            return controllerMap;
        }

        private string GetKeyMapFileXml(ControllerType controllerType)
        {
            var hotbarProfile = (HotbarProfileData)_hotbarData.GetProfile();

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

        //private bool CreateConflictCheck(ElementAssignment assignment, out ElementAssignmentConflictCheck conflictCheck)
        //{
        //    if (mapping == null || _player == null)
        //    {
        //        conflictCheck = new ElementAssignmentConflictCheck();
        //        return false;
        //    }

        //    conflictCheck = assignment.ToElementAssignmentConflictCheck();
        //    conflictCheck.playerId = _player.id;
        //    conflictCheck.controllerType = ControllerType.Keyboard;
        //    conflictCheck.controllerId = 0;
        //    conflictCheck.controllerMapId = ;
        //    conflictCheck.controllerMapCategoryId = RewiredConstants.ActionSlots.CategoryMapId;

        //    return true;
        //}
    }
}
