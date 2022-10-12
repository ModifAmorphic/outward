using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(ItemDisplayDropGround))]
    internal static class ItemDisplayDropGroundPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate bool TryGetIsDropValidDelegate(ItemDisplay draggedDisplay, Character character, out bool result);
        //public static GetIsMenuFocusedDelegate GetIsMenuFocused;
        public static Dictionary<int, TryGetIsDropValidDelegate> TryGetIsDropValids = new Dictionary<int, TryGetIsDropValidDelegate>();
        //public static event GetIsApplicationFocused OnIsApplicationFocused;
        [HarmonyPatch("IsDropValid")]
        [HarmonyPostfix]
        private static void IsDropValidPostfix(ItemDisplay ___m_draggedDisplay, ref bool __result)
        {
            try
            {
                if (___m_draggedDisplay?.LocalCharacter == null || ___m_draggedDisplay.LocalCharacter.OwnerPlayerSys == null || !___m_draggedDisplay.LocalCharacter.IsLocalPlayer)
                    return;


                var playerId = ___m_draggedDisplay.LocalCharacter.OwnerPlayerSys.PlayerID;
                if (TryGetIsDropValids.TryGetValue(playerId, out var tryIsDropValid))
                {
                    if (tryIsDropValid(___m_draggedDisplay, ___m_draggedDisplay.LocalCharacter, out var result))
                        __result = result;
                }

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayDropGroundPatches)}::{nameof(IsDropValidPostfix)}(): Exception Invoking {nameof(TryGetIsDropValids)}.", ex);
            }
        }
    }
}
