using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Patches
{
    [HarmonyPatch(typeof(NetworkLevelLoader))]
    internal static class TransmorphNetworkLevelLoaderPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<NetworkLevelLoader> MidLoadLevelAfter;
        [HarmonyPatch(nameof(NetworkLevelLoader.MidLoadLevel), MethodType.Normal)]
        [HarmonyPostfix]
        private static void MidLoadLevelPrefix(NetworkLevelLoader __instance)
        {
            try
            {
                Logger?.LogTrace($"{nameof(TransmorphNetworkLevelLoaderPatches)}::{nameof(MidLoadLevelPrefix)} called. Invoking {nameof(MidLoadLevelAfter)}.");
                MidLoadLevelAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(TransmorphNetworkLevelLoaderPatches)}::{nameof(MidLoadLevelPrefix)}.", ex);
            }
        }
    }
}
