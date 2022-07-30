﻿using HarmonyLib;
using ModifAmorphic.Outward.ActionMenus.Services;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Logging;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Patches
{
    [HarmonyPatch(typeof(ControlsInput))]
    internal static class ControlsInputPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        /// <summary>
        /// This gets called on Update() so skip raising an event and trace logging. Set the value in the patch.
        /// </summary>
        /// <param name="_playerID"></param>
        /// <param name="_active"></param>
        [HarmonyPatch(nameof(ControlsInput.SetQuickSlotActive))]
        [HarmonyPrefix]
        private static void SetQuickSlotActivePrefix(int _playerID, ref bool _active)
        {
            try
            {
                if (ControlsInput.IsLastActionGamepad(_playerID))
                    return;

                //Logger.LogTrace($"{nameof(ControlsInputPatches)}::{nameof(SetQuickSlotActivePrefix)}(): Invoked. Disabling quickslots for player {_playerID}.");
                if (PlayerMenuService.PlayerMenus.TryGetValue(_playerID, out var menu) && menu.HotbarService != null)
                {
                    ReInput.players.GetPlayer(_playerID).controllers.maps.SetMapsEnabled(_active, ControllerType.Keyboard, RewiredConstants.ActionSlots.CategoryMapId);
                    menu.HotbarService.ToggleEditMode(!_active);
                    _active = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ControlsInputPatches)}::{nameof(SetQuickSlotActivePrefix)}(): Exception sisabling quickslots for player {_playerID}.", ex);
            }
        }
    }
}
