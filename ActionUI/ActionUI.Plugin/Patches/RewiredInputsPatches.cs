using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(RewiredInputs))]
    internal static class RewiredInputsPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        //public static event Action<RewiredInputs> BeforeExportXmlData;

        //[HarmonyPatch(nameof(RewiredInputs.ExportXmlData))]
        //[HarmonyPrefix]
        //private static void ExportXmlDataPrefix(RewiredInputs __instance)
        //{
        //    try
        //    {
        //        Logger.LogTrace($"{nameof(RewiredInputsPatches)}::{nameof(ExportXmlDataPrefix)}(): Invoked. Invoking {BeforeExportXmlData} for player {__instance.PlayerID}.");
        //        BeforeExportXmlData?.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(RewiredInputsPatches)}::{nameof(ExportXmlDataPrefix)}(): Exception invoking {BeforeExportXmlData} for player {__instance.PlayerID}.", ex);
        //    }
        //}


        public static event Action<RewiredInputs> AfterSaveAllMaps;

        [HarmonyPatch(nameof(RewiredInputs.SaveAllMaps))]
        [HarmonyPostfix]
        private static void SaveAllMapsPostfix(RewiredInputs __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(RewiredInputsPatches)}::{nameof(SaveAllMapsPostfix)}(): Invoked. Invoking {AfterSaveAllMaps} for player {__instance.PlayerID}.");
                AfterSaveAllMaps?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(RewiredInputsPatches)}::{nameof(SaveAllMapsPostfix)}(): Exception invoking {AfterSaveAllMaps} for player {__instance.PlayerID}.", ex);
            }
        }

        public delegate void AfterExportXmlDataDelegate(int playerId);
        public static event AfterExportXmlDataDelegate AfterExportXmlData;

        [HarmonyPatch(nameof(RewiredInputs.ExportXmlData))]
        [HarmonyPostfix]
        private static void ExportXmlDataPostfix(RewiredInputs __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(RewiredInputsPatches)}::{nameof(ExportXmlDataPostfix)}(): Invoked. Invoking {AfterExportXmlData} for player {__instance.PlayerID}.");
                AfterExportXmlData?.Invoke(__instance.PlayerID);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(RewiredInputsPatches)}::{nameof(ExportXmlDataPostfix)}(): Exception invoking {AfterExportXmlData} for player {__instance.PlayerID}.", ex);
            }
        }
    }
}
