using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Patches
{
    [HarmonyPatch(typeof(SplitPlayer))]
    internal static class SplitPlayerPatches
    {

        public static event Action<SplitPlayer> InitAfter;

        [HarmonyPatch(nameof(SplitPlayer.Init), MethodType.Normal)]
        [HarmonyPostfix]
        public static void Init(SplitPlayer __instance)
        {
            InitAfter?.Invoke(__instance);
        }

    }
}
