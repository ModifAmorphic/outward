using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Patches
{
    [HarmonyPatch(typeof(CharacterUI))]
    internal static class CharacterUIPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static Func<bool> GetIsMenuFocused;
        //public static event GetIsApplicationFocused OnIsApplicationFocused;
        [HarmonyPatch("IsMenuFocused", MethodType.Getter)]
        [HarmonyPostfix]
        private static void IsMenuFocusedPostfix(CharacterUI __instance, ref bool __result)
        {
            try
            {
                //Logger.LogTrace($"{nameof(MenuManagerPatches)}::{nameof(IsApplicationFocusedPostfix)}(): Invoked. Invoking {nameof(GetIsApplicationFocused)}({nameof(MenuManagerPatches)}).");
                __result = __result || (GetIsMenuFocused?.Invoke() ?? false);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterUIPatches)}::{nameof(IsMenuFocusedPostfix)}(): Exception Invoking {nameof(GetIsMenuFocused)}({nameof(MenuManagerPatches)}).", ex);
            }
        }
    }
}
