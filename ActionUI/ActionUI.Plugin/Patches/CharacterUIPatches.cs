using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UI.Patches
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
                //Logger.LogTrace($"{nameof(MenuManagerPatches)}::{nameof(IsApplicationFocusedPostfix)}(): Invoked. Invoking {nameof(GetIsApplicationFocused)}({nameof(MenuManagerPatches)}).");
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
                Logger.LogException($"{nameof(CharacterUIPatches)}::{nameof(IsMenuFocusedPostfix)}(): Exception Invoking {nameof(GetIsMenuFocused)}({nameof(MenuManagerPatches)}).", ex);
            }
        }
    }
}
