using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(CharacterUI))]
    internal static class CharacterUIPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate bool GetIsMenuFocusedDelegate(int rewiredId);
        //public static GetIsMenuFocusedDelegate GetIsMenuFocused;
        public static Dictionary<int, GetIsMenuFocusedDelegate> GetIsMenuFocused = new Dictionary<int, GetIsMenuFocusedDelegate>();
        //public static event GetIsApplicationFocused OnIsApplicationFocused;
        [HarmonyPatch("IsMenuFocused", MethodType.Getter)]
        [HarmonyPostfix]
        private static void IsMenuFocusedPostfix(CharacterUI __instance, ref bool __result)
        {
            try
            {
                if (__instance.TargetCharacter?.OwnerPlayerSys != null)
                {
                    var playerId = __instance.TargetCharacter.OwnerPlayerSys.PlayerID;
                    if (GetIsMenuFocused.TryGetValue(playerId, out var isMenuFocused))
                    {
                        if (isMenuFocused(playerId))
                            __result = true;
                    }
                    //__result = isFocused;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterUIPatches)}::{nameof(IsMenuFocusedPostfix)}(): Exception Invoking {nameof(GetIsMenuFocused)}.", ex);
            }
        }

        public delegate void ReleaseUIDelegate(CharacterUI characterUI, int rewiredId);
        public static event ReleaseUIDelegate BeforeReleaseUI;
        [HarmonyPatch(nameof(CharacterUI.ReleaseUI))]
        [HarmonyPrefix]
        private static void ReleaseUIPrefix(CharacterUI __instance, int ___m_rewiredID)
        {
            try
            {
                if (___m_rewiredID != -1)
                {
                    Logger.LogTrace($"{nameof(CharacterUIPatches)}::{nameof(ReleaseUIPrefix)}(): Invoking {nameof(BeforeReleaseUI)} for RewiredID {___m_rewiredID}.");
                    BeforeReleaseUI?.Invoke(__instance, ___m_rewiredID);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterUIPatches)}::{nameof(ReleaseUIPrefix)}(): Exception Invoking {nameof(BeforeReleaseUI)} for RewiredID {___m_rewiredID}.", ex);
            }
        }
    }
}
