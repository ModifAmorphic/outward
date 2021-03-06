using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
#if DEBUG
using Rewired;
#endif
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings
{
    [HarmonyPatch]
    internal static class LocalCharacterControlPatches
    {
        private static Dictionary<int, RewiredInputs> _playerInputManager = new Dictionary<int, RewiredInputs>();
        //private static SortedDictionary<int, string> _exQuickSlots = new SortedDictionary<int, string>();
        private static IEnumerable<ExtendedQuickSlot> _exQuickSlots = new List<ExtendedQuickSlot>();

        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        private static void QuickSlotExtenderEvents_SlotsChanged(object sender, QuickSlotExtendedArgs e) => _exQuickSlots = e.ExtendedQuickSlots;


        [EventSubscription]
        public static void SubscribeToEvents()
        {
            //LoggerEvents.LoggerConfigured += LoggerEvents_LoggerLoaded;
            QuickSlotExtenderEvents.SlotsChanged += QuickSlotExtenderEvents_SlotsChanged;
        }

        [HarmonyPatch(typeof(ControlsInput), nameof(ControlsInput.Setup))]
        [HarmonyPostfix]
        public static void OnControlsInputSetup_SetPlayerInputManager()
        {
            _playerInputManager = ((ControlsInput)null).GetPlayerInputManager();

#if DEBUG
            var actionCategories = ReInput.mapping.ActionCategories;
            var logout = string.Empty;
            foreach (var cat in actionCategories)
            {
                logout += $"Category ID: {cat.id}, Name: {cat.name}, DescriptiveName: {cat.descriptiveName}\n";
                var actions = ReInput.mapping.ActionsInCategory(cat.id);
                foreach (var a in actions)
                {
                    logout += $"\tAction Id: {a.id}, Name: {a.name}, DescriptiveName: {a.descriptiveName}\n";
                }
            }
            Logger.LogTrace(logout);
#endif
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
                Logger?.LogTrace($"LocalCharacterControl.UpdateQuickSlots - Character or Character.QuickSlotMng was null.");
                return;
            }

            try
            {
                var playerId = __instance.Character.OwnerPlayerSys.PlayerID;
                //If any of the built in QuickSlot buttons are down, exit.
                if (AnyQuickSlotInstantButtonsDown(playerId))
                {
                    Logger?.LogTrace($"LocalCharacterControl.UpdateQuickSlots - Built in quickslot button was pressed.  Exiting.");
                    return;
                }

                var exQsButtonDown = GetFirstExQuickSlotInstantDown(playerId);
                if (exQsButtonDown > 0)
                {
                    Logger?.LogTrace($"Triggering Ex Quickslot #{exQsButtonDown}");
                    __instance.Character.QuickSlotMngr.QuickSlotInput(exQsButtonDown - 1);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(LocalCharacterControlPatches)}.{nameof(OnUpdateQuickSlots_CheckForButtonPresses)}().", ex);
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
