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

        [HarmonyPatch("BaseLoadLevel", MethodType.Normal)]
        [HarmonyPrefix]
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter
        private static void BaseLoadLevelPrefix(NetworkLevelLoader __instance, int _spawnPoint, float _spawnOffset, bool _save)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            NetworkLevelLoaderEvents.RaiseBaseLoadLevelBefore(__instance, _save);
        }
    }
}
