﻿using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(SplitPlayer))]
    internal static class SplitPlayerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        //public static event Action<SplitPlayer> InitAfter;
        //[HarmonyPatch(nameof(SplitPlayer.Init))]
        //[HarmonyPostfix]
        //private static void InitPostfix(SplitPlayer __instance)
        //{
        //    try
        //    {
        //        if (__instance?.AssignedCharacter == null || __instance.AssignedCharacter.OwnerPlayerSys == null || !__instance.AssignedCharacter.IsLocalPlayer)
        //            return;

        //        Logger.LogTrace($"{nameof(SplitPlayerPatches)}::{nameof(InitPostfix)}(): Invoked. Invoking {nameof(InitAfter)}({nameof(SplitPlayer)}).");
        //        InitAfter?.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(SplitPlayerPatches)}::{nameof(InitPostfix)}(): Exception Invoking {nameof(InitAfter)}({nameof(SplitPlayer)}).", ex);
        //    }
        //}

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

                Logger.LogTrace($"{nameof(SplitPlayerPatches)}::{nameof(SetCharacterPostfix)}(): Invoked. Invoking {nameof(SetCharacterAfter)}({nameof(SplitPlayer)}, {nameof(Character)}).");
                SetCharacterAfter?.Invoke(__instance, _character);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SplitPlayerPatches)}::{nameof(SetCharacterPostfix)}(): Exception Invoking {nameof(SetCharacterAfter)}({nameof(SplitPlayer)}, {nameof(Character)}).", ex);
            }
        }
    }
}
