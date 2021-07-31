using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(ItemManager))]
    internal static class ItemManagerPatches
    {
        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakePostfix(ItemManager __instance)
        {
            ItemManagerEvents.RaiseAwakeAfter(ref __instance);
        }

        [HarmonyPatch("OnReceiveItemSync", MethodType.Normal)]
        [HarmonyPostfix]
        private static void OnReceiveItemSyncPostfix(ItemManager __instance)
        {
            ItemManagerEvents.RaiseOnReceiveItemSyncAfter(__instance);
        }

    }
}
