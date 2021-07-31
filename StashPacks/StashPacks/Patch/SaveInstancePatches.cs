using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using System;
using System.Collections.Generic;
using System.Text;

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
        [HarmonyPatch(nameof(SaveInstance.Save))]
        [HarmonyPostfix]
        private static void SavePostfix(SaveInstance __instance)
        {
            SaveInstanceEvents.RaiseSaveAfter(__instance);
        }
    }
}
