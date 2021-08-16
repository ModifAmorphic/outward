using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(NetworkLevelLoader))]
    internal static class NetworkLevelLoaderPatches
    {

        [HarmonyPatch(nameof(NetworkLevelLoader.MidLoadLevel), MethodType.Normal)]
        [HarmonyPrefix]
        private static void MidLoadLevelPrefix(NetworkLevelLoader __instance)
        {
            NetworkLevelLoaderEvents.RaiseMidLoadLevelBefore(__instance);
        }
    }
}
