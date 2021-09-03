using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(Bag))]
    internal static class BagPatches
    {
        [HarmonyPatch(nameof(Bag.ShowContent), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool ShowContentPrefix(ref Bag __instance, Character _character)
        {
            return BagEvents.HandleShowContentBefore(_character, ref __instance);
        }
    }
}
