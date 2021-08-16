using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(ItemContainer))]
    internal static class ItemContainerPatches
    {

        [HarmonyPatch("RefreshWeight")]
        [HarmonyPostfix]
        private static void RefreshWeightPostfix(ItemContainer __instance)
        {
            ItemContainerEvents.RaiseRefreshWeightAfter(__instance);
        }
    }
}
