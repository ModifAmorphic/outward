using HarmonyLib;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Patches
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
