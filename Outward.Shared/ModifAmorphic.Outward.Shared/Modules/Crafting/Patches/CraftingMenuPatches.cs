using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Modules.Crafting.Patches
{
    [HarmonyPatch(typeof(CraftingMenu))]
    internal static class CraftingMenuPatches
    {

        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

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

        //public static event Action<CustomCraftingMenu> CraftingDoneBefore;
        //[HarmonyPatch("CraftingDone", MethodType.Normal)]
        //[HarmonyPrefix]
        //private static void CraftingDonePrefix(CraftingMenu __instance)
        //{
        //    try
        //    {
        //        if (!(__instance is CustomCraftingMenu customCraftingMenu))
        //        {
        //            Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(CraftingDonePrefix)}(): Menu is not a {nameof(CustomCraftingMenu)}. Not invoking {nameof(CraftingDoneBefore)}.");
        //            return;
        //        }
        //        Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(CraftingDonePrefix)}(): Invoked on CraftingMenu type {customCraftingMenu?.GetType()}. Invoking " +
        //            $"{nameof(CraftingDoneBefore)}({customCraftingMenu?.GetType()})");
        //        CraftingDoneBefore?.Invoke(customCraftingMenu);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(CraftingDonePrefix)}(): Exception Invoking {nameof(CraftingDoneBefore)}().", ex);
        //    }
        //}

        public static event Func<(CustomCraftingMenu CraftingMenu, ItemReferenceQuantity Result, int ResultMultiplier), bool> GenerateResultOverride;
        [HarmonyPatch("GenerateResult", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool GenerateResultPrefix(CraftingMenu __instance, ItemReferenceQuantity _result, int resultMultiplier)
        {
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(GenerateResultPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking {nameof(GenerateResultOverride)}()");

                if (__instance is CustomCraftingMenu customCraftingMenu
                    && (GenerateResultOverride?.Invoke((customCraftingMenu, _result, resultMultiplier)) ?? false))
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

        public static event Func<CustomCraftingMenu, bool> RefreshAvailableIngredientsOverridden;
        [HarmonyPatch("RefreshAvailableIngredients", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool RefreshAvailableIngredientsPrefix(CraftingMenu __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAvailableIngredientsPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}.");
                if (__instance is CustomCraftingMenu menu
                    && (RefreshAvailableIngredientsOverridden?.Invoke(menu) ?? false))
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


        [HarmonyPatch("RefreshAutoIngredients", MethodType.Normal)]
        [HarmonyPrefix]
        private static void RefreshAutoIngredientsPrefix(CraftingMenu __instance, Recipe _recipe, ref IList<int>[] _allMatches, ref IList<int> _bestIngredientsMatch)
        {
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}.");
                if (!(__instance is CustomCraftingMenu customCraftingMenu)
                    || _recipe == null
                    || _recipe.Results == null || _recipe.Results.Length == 0)
                {
                    return;
                }
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Updating MatchIngredientsRecipe to recipe {_recipe.Name}");
                customCraftingMenu.IngredientCraftData.MatchIngredientsRecipe = _recipe;
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPrefix)}(): Exception updating MatchIngredientsRecipe.", ex);
            }
        }
        [HarmonyPatch("RefreshAutoIngredients", MethodType.Normal)]
        [HarmonyPostfix]
        private static void RefreshAutoIngredientsPostfix(CraftingMenu __instance, Recipe _recipe, ref IList<int>[] _allMatches, ref IList<int> _bestIngredientsMatch)
        {
            try
            {
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPostfix)}(): Invoked on CraftingMenu type {__instance?.GetType()}.");
                if (!(__instance is CustomCraftingMenu customCraftingMenu)
                    || _recipe == null
                    || _recipe.Results == null || _recipe.Results.Length == 0)
                {
                    return;
                }
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPostfix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Updating MatchIngredientsRecipe to recipe {_recipe.Name}");
                customCraftingMenu.IngredientCraftData.MatchIngredientsRecipe = null;
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPostfix)}(): Exception nulling MatchIngredientsRecipe.", ex);
            }
        }

        //public delegate void RefreshAutoIngredients(CraftingMenu craftingMenu, Recipe recipe, ref IList<int>[] allMatches, ref IList<int> bestIngredientsMatch);
        //public static event RefreshAutoIngredients RefreshAutoIngredientsAfter;
        //[HarmonyPatch("RefreshAutoIngredients", MethodType.Normal)]
        //[HarmonyPostfix]
        //private static void RefreshAutoIngredientsPostfix(CraftingMenu __instance, Recipe _recipe, ref IList<int>[] _allMatches, ref IList<int> _bestIngredientsMatch)
        //{
        //    try
        //    {
        //        Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPostfix)}(): Invoked on CraftingMenu type {__instance?.GetType()}.");
        //        if (!(__instance is CustomCraftingMenu)
        //            || _recipe == null
        //            || _recipe.Results == null || _recipe.Results.Length == 0
        //            || !(_recipe.Results[0] is DynamicCraftingResult))
        //        {
        //            return;
        //        }
        //        Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPostfix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
        //            $"{nameof(IngredientSelectorHasChangedAfter)}() for recipe {_recipe.Name}");
        //        RefreshAutoIngredientsAfter?.Invoke(__instance, _recipe, ref _allMatches, ref _bestIngredientsMatch);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoIngredientsPostfix)}(): Exception Invoking {nameof(RefreshAutoIngredientsAfter)}().", ex);
        //    }
        //}
    }
}
