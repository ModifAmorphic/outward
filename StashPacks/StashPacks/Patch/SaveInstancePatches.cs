using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(SaveInstance))]
    internal static class SaveInstancePatches
    {
        [HarmonyPatch(nameof(SaveInstance.Save))]
        [HarmonyPrefix]
        private static bool SavePrefix(SaveInstance __instance)
        {

            SaveInstanceEvents.RaiseSaveBefore(__instance);
            return true;
        }
    }
}
