#if DEBUG
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(NetworkLevelLoader))]
    internal class NetworkLevelLoaderPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<NetworkLevelLoader> MidLoadLevelAfter;
        [HarmonyPatch(nameof(NetworkLevelLoader.MidLoadLevel), MethodType.Normal)]
        [HarmonyPostfix]
        private static void MidLoadLevelPostfix(NetworkLevelLoader __instance)
        {
            try
            {
                Logger?.LogTrace($"{nameof(NetworkLevelLoaderPatches)}::{nameof(MidLoadLevelPostfix)} called. Invoking {nameof(MidLoadLevelAfter)}.");
                MidLoadLevelAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(NetworkLevelLoaderPatches)}::{nameof(MidLoadLevelPostfix)}.", ex);
            }
        }

    }
}
#endif