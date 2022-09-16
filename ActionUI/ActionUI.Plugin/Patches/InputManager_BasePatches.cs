using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using Rewired;
using System;

namespace ModifAmorphic.Outward.UI.Patches
{
    [HarmonyPatch(typeof(InputManager_Base))]
    public class InputManager_BasePatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<InputManager_Base> BeforeInitialize;
        [HarmonyPatch("Initialize", MethodType.Normal)]
        [HarmonyPrefix]
        private static void InitializePrefix(InputManager_Base __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(InputManager_BasePatches)}::{nameof(InitializePrefix)}(): Invoked. Invoking {nameof(BeforeInitialize)}(InputManager_Base).");
                BeforeInitialize?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InputManager_BasePatches)}::{nameof(InitializePrefix)}(): Exception Invoking {nameof(BeforeInitialize)}(InputManager_Base).", ex);
            }
        }

        //public static event Action<InputManager_Base> AfterInitialize;
        //[HarmonyPatch("Initialize", MethodType.Normal)]
        //[HarmonyPostfix]
        //private static void InitializePostfix(InputManager_Base __instance)
        //{
        //    try
        //    {
        //        Logger.LogTrace($"{nameof(InputManager_BasePatches)}::{nameof(InitializePostfix)}(): Invoked. Invoking {nameof(AfterInitialize)}(InputManager_Base).");
        //        AfterInitialize?.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(InputManager_BasePatches)}::{nameof(InitializePostfix)}(): Exception Invoking {nameof(AfterInitialize)}(InputManager_Base).", ex);
        //    }
        //}
    }
}
