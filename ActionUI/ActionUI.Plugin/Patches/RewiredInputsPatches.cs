using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.UI.Patches
{
    [HarmonyPatch(typeof(RewiredInputs))]
    internal static class RewiredInputsPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void BeforeExportXmlDataDelegate(int playerId, ref Dictionary<string, string> mappingData);
        public static event BeforeExportXmlDataDelegate BeforeExportXmlData;

        [HarmonyPatch(nameof(RewiredInputs.ExportXmlData))]
        [HarmonyPrefix]
        private static void ExportXmlDataPrefix(RewiredInputs __instance, ref Dictionary<string, string> ___m_mappingData)
        {
            try
            {
                Logger.LogTrace($"{nameof(RewiredInputsPatches)}::{nameof(ExportXmlDataPrefix)}(): Invoked. Invoking {BeforeExportXmlData} for player {__instance.PlayerID}.");
                BeforeExportXmlData?.Invoke(__instance.PlayerID, ref ___m_mappingData);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(RewiredInputsPatches)}::{nameof(ExportXmlDataPrefix)}(): Exception invoking {BeforeExportXmlData} for player {__instance.PlayerID}.", ex);
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
                Logger.LogTrace($"{nameof(RewiredInputsPatches)}::{nameof(ExportXmlDataPrefix)}(): Invoked. Invoking {AfterExportXmlData} for player {__instance.PlayerID}.");
                AfterExportXmlData?.Invoke(__instance.PlayerID);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(RewiredInputsPatches)}::{nameof(ExportXmlDataPrefix)}(): Exception invoking {AfterExportXmlData} for player {__instance.PlayerID}.", ex);
            }
        }
    }
}
