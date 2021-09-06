using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(AreaManager))]
    internal static class AreaManagerPatches
    {
#pragma warning disable IDE0051 // Remove unused private members
        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakePostfix(AreaManager __instance)
        {
            AreaManagerEvents.RaiseAwakeAfter(ref __instance);
        }
    }
}
