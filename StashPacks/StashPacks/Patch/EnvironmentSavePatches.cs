using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(EnvironmentSave))]
    internal static class EnvironmentSavePatches
    {
        [HarmonyPatch(nameof(EnvironmentSave.ApplyData))]
        [HarmonyPrefix]
        private static bool ApplyDataPreFix(ref EnvironmentSave __instance)
        {
            EnvironmentSaveEvents.RaiseApplyDataBefore(ref __instance);
            return true;
        }
        [HarmonyPatch(nameof(EnvironmentSave.ApplyData))]
        [HarmonyPostfix]
        private static void ApplyDataPostFix(EnvironmentSave __instance)
        {
            EnvironmentSaveEvents.RaiseApplyDataAfter(__instance);
        }
    }
}
