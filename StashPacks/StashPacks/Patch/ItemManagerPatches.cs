using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(ItemManager))]
    internal static class ItemManagerPatches
    {
        [HarmonyPatch(nameof(ItemManager.IsAllItemSynced), MethodType.Getter)]
        [HarmonyPostfix]
        private static void IsAllItemSyncedPostfix(ItemManager __instance, ref bool __result)
        {
            ItemManagerEvents.RaiseIsAllItemSyncedAfter(__instance, ref __result);
        }

        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakePostfix(ItemManager __instance)
        {
            ItemManagerEvents.RaiseAwakeAfter(ref __instance);
        }
    }
}
