using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.KeyBindings.Listeners
{
    [HarmonyPatch]
    static class LocalCharacterControlPatches
    {
        private static Dictionary<int, RewiredInputs> _playerInputManager = new Dictionary<int, RewiredInputs>();
        //private static SortedDictionary<int, string> _exQuickSlots = new SortedDictionary<int, string>();
        private static IEnumerable<ExtendedQuickSlot> _exQuickSlots = new List<ExtendedQuickSlot>();
        private static Logging.Logger _logger = new Logging.Logger(Logging.LogLevel.Trace, "ModifAmorphic-Outward");


        private static void LoggerEvents_LoggerLoaded(object sender, Logger logger) => _logger = logger;
        private static void QuickSlotExtenderEvents_SlotsChanged(object sender, QuickSlotExtendedArgs e) => _exQuickSlots = e.ExtendedQuickSlots;

        [EventSubscription]
        public static void SubscribeToEvents()
        {
            LoggerEvents.LoggerLoaded += LoggerEvents_LoggerLoaded;
            QuickSlotExtenderEvents.SlotsChanged += QuickSlotExtenderEvents_SlotsChanged;
        }

        [HarmonyPatch(typeof(ControlsInput), nameof(ControlsInput.Setup))]
        [HarmonyPostfix]
        public static void OnControlsInputSetup_SetPlayerInputManager()
        {
            _playerInputManager = ((ControlsInput)null).GetPlayerInputManager();
        }
        /// <summary>
        /// Hooked method that checks for extended quickslot button presses, and triggers QuickSlotInput on the Character.
        /// </summary>
        [HarmonyPatch(typeof(LocalCharacterControl), "UpdateQuickSlots")]
        [HarmonyPostfix]
        public static void OnUpdateQuickSlots_CheckForButtonPresses(LocalCharacterControl __instance)
        {
            if (_exQuickSlots.Count() < 1)
                return;

            if (__instance.Character == null || __instance.Character.QuickSlotMngr == null)
            {
                _logger?.LogTrace($"LocalCharacterControl.UpdateQuickSlots - Character or Character.QuickSlotMng was null.");
                return;
            }
            
            try
            {
                var playerId = __instance.Character.OwnerPlayerSys.PlayerID;
                //If any of the built in QuickSlot buttons are down, exit.
                if (AnyQuickSlotInstantButtonsDown(playerId))
                {
                    _logger?.LogTrace($"LocalCharacterControl.UpdateQuickSlots - Built in quickslot button was pressed.  Exiting.");
                    return;
                }

                var exQsButtonDown = GetFirstExQuickSlotInstantDown(playerId);
                if (exQsButtonDown > 0)
                {
                    _logger?.LogTrace($"Triggering Ex Quickslot #{exQsButtonDown}");
                    __instance.Character.QuickSlotMngr.QuickSlotInput(exQsButtonDown - 1);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogException($"Exception in {nameof(QuickSlotExtender)}.{nameof(OnUpdateQuickSlots_CheckForButtonPresses)}().", ex);
                throw;
            }
        }
        public static bool AnyQuickSlotInstantButtonsDown(int _playerID)
        {
            return ControlsInput.QuickSlotInstant1(_playerID) || ControlsInput.QuickSlotInstant2(_playerID) || ControlsInput.QuickSlotInstant3(_playerID) || ControlsInput.QuickSlotInstant4(_playerID)
                | ControlsInput.QuickSlotInstant5(_playerID) || ControlsInput.QuickSlotInstant6(_playerID) || ControlsInput.QuickSlotInstant7(_playerID) || ControlsInput.QuickSlotInstant8(_playerID);
        }
        public static int GetFirstExQuickSlotInstantDown(int playerID)
        {
            foreach (var slot in _exQuickSlots)
            {
                if (QuickSlotInstantN(playerID, slot.ActionName))
                    return slot.QuickSlotId;
            }
            return -1;
        }
        public static bool QuickSlotInstantN(int playerId, string actionName)
        {
            return _playerInputManager[playerId].GetButtonDown(actionName);
        }
    }
}
