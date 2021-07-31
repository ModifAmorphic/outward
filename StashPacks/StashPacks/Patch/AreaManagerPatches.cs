using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(AreaManager))]
    internal static class AreaManagerPatches
    {
        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakePostfix(AreaManager __instance)
        {
            AreaManagerEvents.RaiseAwakeAfter(ref __instance);
        }
    }
}
