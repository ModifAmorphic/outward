using BepInEx;
using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionMenus.Models;
using Rewired;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class ControllerMapService
    {
        private readonly HotbarSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly HotkeyCaptureDialog _captureDialog;
        private readonly HotbarProfileJsonService _profileData;
        private readonly HotbarService _hotbarService;
        private readonly Player _player;
        private readonly LevelCoroutines _coroutine;

        private const string ActionSlotsPlayer0Key = "RewiredData&amp;playerName=Player0&amp;dataType=ControllerMap&amp;controllerMapType=KeyboardMap&amp;categoryId=131000&amp;layoutId=0&amp;hardwareIdentifier=Keyboard";
        private const string ActionSlotsPlayer1Key = "RewiredData&amp;playerName=Player1&amp;dataType=ControllerMap&amp;controllerMapType=KeyboardMap&amp;categoryId=131000&amp;layoutId=0&amp;hardwareIdentifier=Keyboard";
        private const string KeyboardMapFile = "KeyboardMap_ActionSlots.xml";

        public ControllerMapService(HotkeyCaptureDialog captureDialog,
                                HotbarProfileJsonService profileData,
                                HotbarService hotbarService,
                                Player player,
                                LevelCoroutines coroutine,
                                HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            (_captureDialog, _profileData, _hotbarService, _player, _coroutine, _settings, _getLogger) = (captureDialog, profileData, hotbarService, player, coroutine, settings, getLogger);

            RewiredInputsPatches.BeforeExportXmlData += RemoveActionMenusMaps;
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

        private void RemoveActionMenusMaps(int playerId, ref Dictionary<string, string> mappingData)
        {
            if (playerId != _player.id)
                return;

            if (playerId == 0 && mappingData.ContainsKey(ActionSlotsPlayer0Key))
                mappingData.Remove(ActionSlotsPlayer0Key);

            if (playerId == 1 && mappingData.ContainsKey(ActionSlotsPlayer1Key))
                mappingData.Remove(ActionSlotsPlayer1Key);
        }

        public void LoadConfigMaps()
        {
            GetKeyboardMap();
        }

        private void CaptureDialog_OnKeysSelected(int id, HotkeyCategories category, KeyGroup keyGroup)
        {
            
            var map = GetKeyboardMap();
            if (category != HotkeyCategories.ActionSlot)
                return;

            Logger.LogDebug($"Setting ActionSlot Hotkey for Slot Index {id}.");

            var profile = _profileData.GetActiveProfile();
            var hotbars = profile.Hotbars;
            var config = (ActionConfig)hotbars[0].Slots[id].Config;
            var eleMap = new ElementAssignment(keyGroup.KeyCode, GetModifierKeyFlags(keyGroup.Modifiers), config.RewiredActionId, Pole.Positive);

            var existingMaps = map.ElementMapsWithAction(config.RewiredActionId).ToArray();
            
            if (existingMaps.Any() && existingMaps.Length > 1)
            {
                for (int i = 1; i < existingMaps.Length; i++)
                    map.DeleteElementMap(existingMaps[i].id);
            }
            if (existingMaps.Any())
            {
                eleMap.elementMapId = existingMaps.First().id;
                eleMap.elementIdentifierId = existingMaps.First().elementIdentifierId;
            }

            map.ReplaceOrCreateElementMap(eleMap);

            var hotkey = map.ButtonMaps.FirstOrDefault(m => m.actionId == config.RewiredActionId).elementIdentifierName;
            for (int b = 0; b < hotbars.Count; b++)
            {
                hotbars[b].Slots[id].Config.HotkeyText = hotkey;
            }

            _profileData.SaveProfile(profile);
            _hotbarService.ConfigureHotbars(profile);
            SaveKeyboardMap(map);
        }

        private void SaveKeyboardMap(KeyboardMap keyboardMap)
        {
            var mapXml = keyboardMap.ToXmlString();
            var filePath = Path.Combine(GetProfileFolder(), KeyboardMapFile);
            File.WriteAllText(filePath, mapXml);
        }

        private KeyboardMap GetKeyboardMap()
        {
            var map = _player.controllers.maps.GetMap<KeyboardMap>(0, RewiredConstants.ActionSlots.CategoryMapId, 0);
            if (map != null)
            {
                Logger.LogDebug($"ActionSlots KeyboardMap found for player {_player.id}");
                return map;
            }
            var activeProfile = (ProfileData)_profileData.GetActiveProfile();
            var keyMapFile = RewiredConstants.ActionSlots.DefaultKeyboardMapFile;
            if (activeProfile != null)
            {
                var profileDir = _profileData.GetOrAddProfileDir(activeProfile.Name);
                keyMapFile = Path.Combine(profileDir, KeyboardMapFile);

                if (!File.Exists(keyMapFile))
                    File.Copy(RewiredConstants.ActionSlots.DefaultKeyboardMapFile, keyMapFile);
            }

            var xml = File.ReadAllText(keyMapFile);
            var keyboardMap = (KeyboardMap)ControllerMap.CreateFromXml(ControllerType.Keyboard, xml);
            _player.controllers.maps.AddMap<KeyboardMap>(0, keyboardMap);
            Logger.LogDebug($"Loaded ActionSlots KeyboardMap found for player {_player.id} at location '{keyMapFile}'.");

            return keyboardMap;
        }

        private string GetProfileFolder()
        {
            var activeProfile = (ProfileData)_profileData.GetActiveProfile();
            return _profileData.GetOrAddProfileDir(activeProfile.Name);
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
