//using HarmonyLib;
//using ModifAmorphic.Outward.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches
//{
//    [HarmonyPatch(typeof(LedgerMenu))]
//    internal static class LedgerMenuPatches
//    {
//        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

//        public static event Action<LedgerMenu> BeforeStartInit;

//        [HarmonyPatch("StartInit", MethodType.Normal)]
//        [HarmonyPatch(new Type[] { })]
//        [HarmonyPrefix]
//        private static void StartInitPrefix(LedgerMenu __instance)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(LedgerMenuPatches)}::{nameof(StartInitPrefix)}(): Invoking {nameof(BeforeStartInit)}.");
//#endif
//                BeforeStartInit?.Invoke(__instance);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(LedgerMenuPatches)}::{nameof(StartInitPrefix)}(): Exception invoking {nameof(BeforeStartInit)}.", ex);
//            }
//        }

//        public static event Action<LedgerMenu> AfterShow;

//        [HarmonyPatch("Show", MethodType.Normal)]
//        [HarmonyPatch(new Type[] { typeof(Item) })]
//        [HarmonyPostfix]
//        private static void ShowPostfix(LedgerMenu __instance)
//        {
//            try
//            {
//#if DEBUG
//                Logger.LogTrace($"{nameof(LedgerMenuPatches)}::{nameof(ShowPostfix)}(): Invoking {nameof(AfterShow)}.");
//#endif
//                AfterShow?.Invoke(__instance);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(LedgerMenuPatches)}::{nameof(ShowPostfix)}(): Exception invoking {nameof(AfterShow)}.", ex);
//            }
//        }
//    }
//}
