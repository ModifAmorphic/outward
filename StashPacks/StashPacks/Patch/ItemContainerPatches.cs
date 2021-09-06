using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(ItemContainer))]
    internal static class ItemContainerPatches
    {
#pragma warning disable IDE0051 // Remove unused private members
        [HarmonyPatch("RefreshWeight")]
        [HarmonyPostfix]
        private static void RefreshWeightPostfix(ItemContainer __instance)
        {
            ItemContainerEvents.RaiseRefreshWeightAfter(__instance);
        }
    }
}
