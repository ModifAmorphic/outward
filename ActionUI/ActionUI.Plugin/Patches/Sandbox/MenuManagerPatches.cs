#if DEBUG
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches.Sandbox
{
    [HarmonyPatch(typeof(MenuManager))]
    internal static class MenuManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        //public static event Action<MenuManager> AwakeBefore;
        //[HarmonyPatch("Awake")]
        //[HarmonyPrefix]
        //private static void AwakePrefix(MenuManager __instance)
        //{
        //    try
        //    {
        //        Logger.LogTrace($"{nameof(MenuManagerPatches)}::{nameof(AwakePrefix)}(): Invoked. Invoking {nameof(AwakePrefix)}({nameof(MenuManagerPatches)}).");
        //        AwakeBefore?.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(MenuManagerPatches)}::{nameof(AwakePrefix)}(): Exception Invoking {nameof(AwakePrefix)}({nameof(MenuManagerPatches)}).", ex);
        //    }
        //}

        //public delegate bool GetIsApplicationFocused(MenuManager __instance);
        //public static GetIsApplicationFocused GetIsApplicationFocusedCallback;
        public static event Func<bool> GetIsApplicationFocused;
        //public static event GetIsApplicationFocused OnIsApplicationFocused;
        [HarmonyPatch("IsApplicationFocused", MethodType.Getter)]
        [HarmonyPostfix]
        private static void IsApplicationFocusedPostfix(MenuManager __instance, ref bool __result)
        {
            try
            {
                //Logger.LogTrace($"{nameof(MenuManagerPatches)}::{nameof(IsApplicationFocusedPostfix)}(): Invoked. Invoking {nameof(GetIsApplicationFocused)}({nameof(MenuManagerPatches)}).");
                __result = __result || (GetIsApplicationFocused?.Invoke() ?? false);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(MenuManagerPatches)}::{nameof(IsApplicationFocusedPostfix)}(): Exception Invoking {nameof(GetIsApplicationFocused)}({nameof(MenuManagerPatches)}).", ex);
            }
        }
    }
}
#endif