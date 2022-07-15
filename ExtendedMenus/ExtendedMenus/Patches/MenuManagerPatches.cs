using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal static class MenuManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<MenuManager> AwakeBefore;
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        private static void AwakePrefix(MenuManager __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(MenuManagerPatches)}::{nameof(AwakePrefix)}(): Invoked. Invoking {nameof(AwakePrefix)}({nameof(MenuManagerPatches)}).");
                AwakeBefore?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MenuManagerPatches)}::{nameof(AwakePrefix)}(): Exception Invoking {nameof(AwakePrefix)}({nameof(MenuManagerPatches)}).", ex);
            }
        }
    }
}
