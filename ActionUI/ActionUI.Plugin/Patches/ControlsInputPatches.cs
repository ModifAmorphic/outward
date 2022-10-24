using HarmonyLib;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using Rewired;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(ControlsInput))]
    internal static class ControlsInputPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        /// <summary>
        /// Activates Action Slots Rewired Maps and disables Outwards Quickslot Keyboard Maps
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
                if (Psp.Instance.GetServicesProvider(_playerID).TryGetService<HotbarsContainer>(out var hotbars) && hotbars.IsAwake)
                {
                    ReInput.players.GetPlayer(_playerID).controllers.maps.SetMapsEnabled(_active, ControllerType.Keyboard, RewiredConstants.ActionSlots.CategoryMapId);
                    hotbars.Controller.ToggleActionSlotEdits(!_active);
                    _active = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ControlsInputPatches)}::{nameof(SetQuickSlotActivePrefix)}(): Exception disabling quickslots for player {_playerID}.", ex);
            }
        }
    }
}
