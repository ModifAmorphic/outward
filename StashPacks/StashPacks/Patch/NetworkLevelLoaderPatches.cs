using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(NetworkLevelLoader))]
    internal static class NetworkLevelLoaderPatches
    {
        //[HarmonyPatch(nameof(NetworkLevelLoader.OnJoinedRoom), MethodType.Normal)]
        //[HarmonyPrefix]
        //private static void OnJoinedRoomPrefix(NetworkLevelLoader __instance, bool ___m_failedJoin)
        //{
        //    NetworkLevelLoaderEvents.RaiseOnJoinedRoomBefore(__instance, ___m_failedJoin);
        //}

        [HarmonyPatch(nameof(NetworkLevelLoader.MidLoadLevel), MethodType.Normal)]
        [HarmonyPrefix]
        private static void MidLoadLevelPrefix(NetworkLevelLoader __instance)
        {
            NetworkLevelLoaderEvents.RaiseMidLoadLevelBefore(__instance);
        }
    }
}
