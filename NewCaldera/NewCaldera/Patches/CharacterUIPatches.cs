using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.NewCaldera.Patches
{
    [HarmonyPatch(typeof(CharacterUI))]
    internal static class CharacterUIPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

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
#if DEBUG
                    Logger.LogTrace($"{nameof(CharacterUIPatches)}::{nameof(ReleaseUIPrefix)}(): Invoking {nameof(BeforeReleaseUI)} for RewiredID {___m_rewiredID}.");
#endif
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
