//using HarmonyLib;
//using ModifAmorphic.Outward.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Patches
//{
//    [HarmonyPatch(typeof(Panel))]
//    internal static class PanelPatches
//    {
//        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

//        public static event Action<LedgerMenu> BeforeLedgerMenuAwakeInit;

//        [HarmonyPatch("AwakeInit", MethodType.Normal)]
//        [HarmonyPatch(new Type[] { })]
//        [HarmonyPrefix]
//        private static void AwakeInitPrefix(Panel __instance)
//        {
//            try
//            {
//                if (!(__instance is LedgerMenu ledger))
//                    return;
//#if DEBUG
//                Logger.LogTrace($"{nameof(PanelPatches)}::{nameof(AwakeInitPrefix)}(): Invoking {nameof(BeforeLedgerMenuAwakeInit)}.");
//#endif
//                BeforeLedgerMenuAwakeInit?.Invoke(ledger);
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(PanelPatches)}::{nameof(AwakeInitPrefix)}(): Exception invoking {nameof(BeforeLedgerMenuAwakeInit)}.", ex);
//            }
//        }
//    }
//}
