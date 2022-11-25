using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
{
    [HarmonyPatch(typeof(AISquad))]
    internal static class AISquadPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void SetSquadActiveDelegate(AISquad squad, bool active, bool resetPositions);
        public static event SetSquadActiveDelegate BeforeSetSquadActive;

        [HarmonyPatch(nameof(AISquad.SetSquadActive), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        private static void SetSquadActivePrefix(AISquad __instance, bool _active, bool _resetPositions = true)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(AISquadPatches)}::{nameof(SetSquadActivePrefix)}(_active: {_active}, _resetPositions: {_resetPositions}): Invoking {nameof(BeforeSetSquadActive)} for squad {__instance.name}.");
#endif
                BeforeSetSquadActive?.Invoke(__instance, _active, _resetPositions);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(AISquadPatches)}::{nameof(SetSquadActivePrefix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
            }
        }

        [HarmonyPatch("OnDisable", MethodType.Normal)]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPrefix]
        private static void OnDisablePrefix(AISquad __instance)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(AISquadPatches)}::{nameof(OnDisablePrefix)}() AISquad: {__instance.name}.");
#endif
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(AISquadPatches)}::{nameof(OnDisablePrefix)}(): Exception invoking {nameof(BeforeSetSquadActive)}.", ex);
            }

            //throw new Exception($"{nameof(AISquadPatches)}::{nameof(OnDisablePrefix)}() AISquad: {__instance.name}. Throwing exception for stack trace.");
        }
    }
}
