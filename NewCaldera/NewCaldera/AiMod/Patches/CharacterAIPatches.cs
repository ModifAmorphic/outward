using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
{
    [HarmonyPatch(typeof(CharacterAI))]
    internal static class CharacterAIPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);
        private static int disableAttempts = 0;

        [HarmonyPatch("OnDisable", MethodType.Normal)]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPrefix]
        private static void OnDisablePrefix(CharacterAI __instance)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(CharacterAIPatches)}::{nameof(OnDisablePrefix)}() CharacterAI: {__instance.name}.");
#endif
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterAIPatches)}::{nameof(OnDisablePrefix)}(): Exception.", ex);
            }

            //if (__instance.Character.UID == "6sB4_5lOJU2bWuMHnOL4Ww")
            //    throw new Exception($"{nameof(CharacterAIPatches)}::{nameof(OnDisablePrefix)}() CharacterAI: {__instance.name}. Throwing exception for stack trace.");
            disableAttempts++;
        }

        [HarmonyPatch("OnEnable", MethodType.Normal)]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPrefix]
        private static void OnEnablePrefix(CharacterAI __instance)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(CharacterAIPatches)}::{nameof(OnEnablePrefix)}() CharacterAI: {__instance.name}.");
#endif
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterAIPatches)}::{nameof(OnEnablePrefix)}(): Exception.", ex);
            }

            //if (__instance.Character.UID == "6sB4_5lOJU2bWuMHnOL4Ww" && disableAttempts == 1)
            //    throw new Exception($"{nameof(CharacterAIPatches)}::{nameof(OnEnablePrefix)}() CharacterAI: {__instance.name}. Throwing exception for stack trace.");
            //disableAttempts++;
        }
    }
}
