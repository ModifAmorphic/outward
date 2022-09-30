using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using Rewired.Data;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches.Sandbox
{
    [HarmonyPatch(typeof(UserDataStore_PlayerPrefs))]
    public class UserDataStore_PlayerPrefsPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        [HarmonyPatch("OnInitialize", MethodType.Normal)]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPostfix]
        public static void OnInitializePostfix(UserDataStore_PlayerPrefsPatches __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(UserDataStore_PlayerPrefsPatches)}::{nameof(OnInitializePostfix)}(): Invoked. Adding new ActionCategory and Actions");

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(UserDataStore_PlayerPrefsPatches)}::{nameof(OnInitializePostfix)}(): Exception", ex);
            }
        }
    }
}
