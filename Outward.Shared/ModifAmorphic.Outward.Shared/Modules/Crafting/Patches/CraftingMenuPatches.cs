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

        public static event Func<(CraftingMenu CraftingMenu, ItemReferenceQuantity Result, int ResultMultiplier), bool> GenerateResultOverride;
        [HarmonyPatch("GenerateResult", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool GenerateResultPrefix(CraftingMenu __instance, ItemReferenceQuantity _result, int resultMultiplier)
        {
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(GenerateResultPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking {nameof(GenerateResultOverride)}()");

                if (__instance is CustomCraftingMenu 
                    && (GenerateResultOverride ?.Invoke((__instance, _result, resultMultiplier)) ?? false))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(GenerateResultPrefix)}(): {nameof(GenerateResultOverride)}() result: was true. Returning false to override base method.");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(GenerateResultPrefix)}(): Exception Invoking {nameof(GenerateResultOverride)}().", ex);
            }
            Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(GenerateResultPrefix)}(): {nameof(GenerateResultOverride)}() result: was false. Returning true and continuing base method invocation.");
            return true;
        }

        //public static event Action<bool> SetCraftButtonEnableBefore;
        //[HarmonyPatch("SetCraftButtonEnable", MethodType.Normal)]
        //[HarmonyPrefix]
        //private static void SetCraftButtonEnablePrefix(CraftingMenu __instance, bool _enable)
        //{
        //    try
        //    {
        //        if (!(__instance is CustomCraftingMenu))
        //        {
        //            Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(SetCraftButtonEnablePrefix)}(): Menu is not a {nameof(CustomCraftingMenu)}. Not invoking {nameof(SetCraftButtonEnableBefore)}.");
        //            return;
        //        }
        //        Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(SetCraftButtonEnablePrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
        //            $"{nameof(SetCraftButtonEnableBefore)}({_enable})");
        //        SetCraftButtonEnableBefore?.Invoke(_enable);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPostfix)}(): Exception Invoking {nameof(OnRecipeSelectedAfter)}().", ex);
        //    }
        //}

        public static event Action<(CraftingMenu CraftingMenu, int Index, bool ForceRefresh)> OnRecipeSelectedAfter;
        [HarmonyPatch(nameof(CraftingMenu.OnRecipeSelected), MethodType.Normal)]
        [HarmonyPostfix]
        private static void OnRecipeSelectedPostfix(CraftingMenu __instance, int _index, bool _forceRefresh = false)
        {
            try
            {
                if (!(__instance is CustomCraftingMenu))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPostfix)}(): Menu is not a {nameof(CustomCraftingMenu)}. Not invoking {nameof(OnRecipeSelectedAfter)}.");
                    return;
                }
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPostfix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
                    $"{nameof(OnRecipeSelectedAfter)}({_index}, {_forceRefresh})");
                OnRecipeSelectedAfter?.Invoke((__instance, _index, _forceRefresh));
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPostfix)}(): Exception Invoking {nameof(OnRecipeSelectedAfter)}().", ex);
            }
        }

        public static event Action<(CraftingMenu CraftingMenu, int SelectorIndex, int ItemID)> IngredientSelectorHasChangedAfter;
        [HarmonyPatch(nameof(CraftingMenu.IngredientSelectorHasChanged), MethodType.Normal)]
        [HarmonyPostfix]
        private static void IngredientSelectorHasChangedPostfix(CraftingMenu __instance, int _selectorIndex, int _itemID)
        {
            try
            {
                if (!(__instance is CustomCraftingMenu))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(IngredientSelectorHasChangedPostfix)}(): Menu is not a {nameof(CustomCraftingMenu)}. Not invoking {nameof(IngredientSelectorHasChangedAfter)}.");
                    return;
                }
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(IngredientSelectorHasChangedPostfix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
                    $"{nameof(IngredientSelectorHasChangedAfter)}({_selectorIndex}, {_itemID})");
                IngredientSelectorHasChangedAfter?.Invoke((__instance, _selectorIndex, _itemID));
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(IngredientSelectorHasChangedPostfix)}(): Exception Invoking {nameof(IngredientSelectorHasChangedAfter)}().", ex);
            }
        }

        //public static event Func<(CraftingMenu CraftingMenu, int Index, bool ForceRefresh), bool> OnRecipeSelectedOverride;
        //[HarmonyPatch(nameof(CraftingMenu.OnRecipeSelected), MethodType.Normal)]
        //[HarmonyPrefix]
        //private static bool OnRecipeSelectedPrefix(CraftingMenu __instance, int _index, bool _forceRefresh = false)
        //{
        //    try
        //    {
        //        Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
        //            $"{nameof(OnRecipeSelectedPrefix)}({_index}, {_forceRefresh})");

        //        if (__instance is CustomCraftingMenu
        //            && (OnRecipeSelectedOverride?.Invoke((__instance, _index, _forceRefresh)) ?? false))
        //        {
        //            Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPrefix)}(): {nameof(OnRecipeSelectedOverride)}() result: was true. Returning false to override base method.");
        //            return false;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPrefix)}(): Exception Invoking {nameof(OnRecipeSelectedOverride)}().", ex);
        //    }
        //    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPrefix)}(): {nameof(OnRecipeSelectedOverride)}() result: was false. Returning true and continuing base method invocation.");
        //    return true;
        //}

        public static event Func<CraftingMenu, bool> RefreshAvailableIngredientsOverridden;
        [HarmonyPatch("RefreshAvailableIngredients", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool RefreshAvailableIngredientsPrefix(CraftingMenu __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAvailableIngredientsPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}.");
                if (__instance is CustomCraftingMenu && (RefreshAvailableIngredientsOverridden?.Invoke(__instance) ?? false))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAvailableIngredientsPrefix)}(): {nameof(RefreshAvailableIngredientsOverridden)}() result: was true. Returning false to override base method.");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAvailableIngredientsPrefix)}(): Exception Invoking {nameof(RefreshAvailableIngredientsOverridden)}({__instance?.GetType()}).", ex);
            }
            return true;
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
