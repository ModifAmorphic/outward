using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph
{
    [HarmonyPatch(typeof(CraftingMenu))]
    internal static class CraftingMenuPatches
    {
        public static event Action<CraftingMenu> AwakeBefore;
        [HarmonyPatch("AwakeInit", MethodType.Normal)]
        [HarmonyPrefix]
        private static void AwakeInitPrefix(CraftingMenu __instance)
        {
            AwakeBefore?.Invoke(__instance);
        }

        public static event Action<CraftingMenu> AwakeAfter;
        [HarmonyPatch("AwakeInit", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakeInitPostfix(CraftingMenu __instance)
        {
            AwakeAfter?.Invoke(__instance);
        }

        public static event Action<CraftingMenu> ShowBefore;
        [HarmonyPatch(nameof(CraftingMenu.Show), MethodType.Normal)]
        [HarmonyPatch(new Type[] { })]
        private static void ShowPrefix(CraftingMenu __instance)
        {
            ShowBefore?.Invoke(__instance);
        }
    }
}
