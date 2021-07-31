using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(Bag))]
    internal static class BagPatches
    {
        
        [HarmonyPatch(nameof(Bag.BagDropCast), MethodType.Normal)]
        [HarmonyPrefix]
        [HarmonyPatch(new Type[] { typeof(bool) })]
        private static bool BagDropCastPrefix(ref Bag __instance)
        {

            BagEvents.RaiseBagDropCastBefore(ref __instance);

            return true;
        }
    }
}
