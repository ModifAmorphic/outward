using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(PauseMenu))]
    internal static class PauseMenuPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<PauseMenu> AfterRefreshDisplay;

        [HarmonyPatch(nameof(PauseMenu.RefreshDisplay))]
        [HarmonyPostfix]
        private static void RefreshDisplayPostfix(PauseMenu __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(PauseMenu)}::{nameof(RefreshDisplayPostfix)}(): Invoked. Invoking {nameof(AfterRefreshDisplay)}.");
                AfterRefreshDisplay?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(PauseMenu)}::{nameof(RefreshDisplayPostfix)}(): Exception invoking {nameof(AfterRefreshDisplay)}.", ex);
            }
        }
    }
}
