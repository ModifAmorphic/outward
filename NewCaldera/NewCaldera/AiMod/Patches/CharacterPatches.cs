using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Patches
{
    [HarmonyPatch(typeof(Character))]
    internal static class CharacterPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void DieDelegate(Character character, bool loadedDead);
        public static event DieDelegate BeforeDie;
        public static event DieDelegate AfterDie;

        [HarmonyPatch(nameof(Character.Die), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(Vector3), typeof(bool) })]
        [HarmonyPrefix]
        private static void DiePrefix(Character __instance, Vector3 _hitVec, bool _loadedDead = false)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(CharacterPatches)}::{nameof(DiePrefix)}() Character: {__instance.name}.");
#endif
                BeforeDie?.Invoke(__instance, _loadedDead);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterPatches)}::{nameof(DiePrefix)}(): Exception.", ex);
            }
        }

        [HarmonyPatch(nameof(Character.Die), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(Vector3), typeof(bool) })]
        [HarmonyPostfix]
        private static void DiePostfix(Character __instance, Vector3 _hitVec, bool _loadedDead = false)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(CharacterPatches)}::{nameof(DiePostfix)}() Character: {__instance.name}.");
#endif
                AfterDie?.Invoke(__instance, _loadedDead);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterPatches)}::{nameof(DiePostfix)}(): Exception.", ex);
            }
        }
    }
}
