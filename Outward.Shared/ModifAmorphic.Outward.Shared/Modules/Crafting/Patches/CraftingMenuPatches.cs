using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    [HarmonyPatch(typeof(CraftingMenu))]
    internal static class CraftingMenuPatches
    {

        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<CraftingMenu> AwakeInitBefore;
        [HarmonyPatch("AwakeInit", MethodType.Normal)]
        [HarmonyPrefix]
        private static void AwakeInitPrefix(CraftingMenu __instance)
        {
            
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(AwakeInitPrefix)}(): Invoked. Invoking {nameof(AwakeInitBefore)}({nameof(CraftingMenu)})");
                AwakeInitBefore?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(AwakeInitPrefix)}(): Exception Invoking {nameof(AwakeInitBefore)}({nameof(CraftingMenu)}).", ex);
            }
        }

        public static event Action<CraftingMenu> AwakeInitAfter;
        [HarmonyPatch("AwakeInit", MethodType.Normal)]
        [HarmonyPostfix]
        private static void AwakeInitPostfix(CraftingMenu __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(AwakeInitPostfix)}(): Invoked. Invoking {nameof(AwakeInitAfter)}({nameof(CraftingMenu)})");
                AwakeInitAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(AwakeInitPostfix)}(): Exception Invoking {nameof(AwakeInitAfter)}({nameof(CraftingMenu)}).", ex);
            }
        }

        public static event Func<CraftingMenu, bool> RefreshAvailableIngredientsOverridden;
        [HarmonyPatch("RefreshAvailableIngredients", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool RefreshAvailableIngredientsPrefix(CraftingMenu __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(AwakeInitPrefix)}(): Invoked. Invoking {nameof(AwakeInitBefore)}({nameof(CraftingMenu)})");
                if (RefreshAvailableIngredientsOverridden?.Invoke(__instance)??false)
                    return false;

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(AwakeInitPrefix)}(): Exception Invoking {nameof(AwakeInitBefore)}({nameof(CraftingMenu)}).", ex);
            }
            return true;
        }

        //public static event Action<CraftingMenu> ShowBefore;
        //[HarmonyPatch(nameof(CraftingMenu.Show), MethodType.Normal)]
        //[HarmonyPatch(new Type[] { })]
        //private static void ShowPrefix(CraftingMenu __instance)
        //{
        //    ShowBefore?.Invoke(__instance);
        //}
    }
}
