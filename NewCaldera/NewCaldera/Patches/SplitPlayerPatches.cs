using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.Patches
{
    [HarmonyPatch(typeof(SplitPlayer))]
    internal static class SplitPlayerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void SetCharacter(SplitPlayer splitPlayer, Character character);
        public static event SetCharacter SetCharacterAfter;

        [HarmonyPatch(nameof(SplitPlayer.SetCharacter))]
        [HarmonyPatch(new Type[] { typeof(Character) })]
        [HarmonyPostfix]
        private static void SetCharacterPostfix(SplitPlayer __instance, Character _character)
        {
            try
            {
                if (__instance?.AssignedCharacter == null || __instance.AssignedCharacter.OwnerPlayerSys == null || !__instance.AssignedCharacter.IsLocalPlayer)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(SplitPlayerPatches)}::{nameof(SetCharacterPostfix)}(): Invoked. Invoking {nameof(SetCharacterAfter)}({nameof(SplitPlayer)}, {nameof(Character)}).");
#endif
                SetCharacterAfter?.Invoke(__instance, _character);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SplitPlayerPatches)}::{nameof(SetCharacterPostfix)}(): Exception Invoking {nameof(SetCharacterAfter)}({nameof(SplitPlayer)}, {nameof(Character)}).", ex);
            }
        }
    }
}
