using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(ItemContainer))]
    internal static class ItemContainerPatches
    {

        //RemoveStackAmount(Item _item, int _quantity)
        //[HarmonyPatch(nameof(ItemContainer.RemoveStackAmount))]
        //[HarmonyPrefix]
        //private static bool RemoveStackAmountPrefix(ItemContainer __instance)
        //{
        //    ItemContainerStaticEvents.RemoveStackAmountBefore(__instance);
        //    return true;
        //}
        //[HarmonyPatch(nameof(ItemContainer.RemoveStackAmount))]
        //[HarmonyPostfix]
        //private static void RemoveStackAmountPostfix(ItemContainer __instance)
        //{
        //    ItemContainerEvents.RaiseRemoveStackAmountAfter(__instance);
        //}

        [HarmonyPatch("RefreshWeight")]
        [HarmonyPostfix]
        private static void RefreshWeightPostfix(ItemContainer __instance)
        {
            ItemContainerEvents.RaiseRefreshWeightAfter(__instance);
        }
    }
}
