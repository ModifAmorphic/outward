using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Modules.Crafting.Patches
{
    [HarmonyPatch(typeof(CraftingMenu))]
    internal static class CraftingMenuPatches
    {

        [MultiLogger]
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

        //[HarmonyPatch(nameof(CraftingMenu.IsSurvivalCrafting), MethodType.Getter)]
        //[HarmonyPostfix]
        //private static void IsSurvivalCraftingPostfix(CraftingMenu __instance, ref bool __result)
        //{
        //    try
        //    {
        //        var menuType = __instance?.GetType();
        //        if (!(__instance is CustomCraftingMenu customMenu))
        //        {
        //            Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(IsSurvivalCraftingPostfix)}(): Menu {menuType} is not a {nameof(CustomCraftingMenu)}.");
        //            return;
        //        }
        //        Logger.LogDebug($"{nameof(CraftingMenuPatches)}::{nameof(IsSurvivalCraftingPostfix)}(): Invoked on CraftingMenu type {menuType}. Invoking " +
        //            $"{nameof(CustomCraftingMenu.TryGetIsSurvivalCrafting)}()");
        //        //if (customMenu.TryGetIsSurvivalCrafting(out var isSurvivalCrafting))
        //        //    __result = isSurvivalCrafting;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(IsSurvivalCraftingPostfix)}(): Exception Invoking {nameof(CustomCraftingMenu.TryGetIsSurvivalCrafting)}().", ex);
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

        [HarmonyPatch(nameof(CraftingMenu.OnRecipeSelected), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool OnRecipeSelectedPrefix(CraftingMenu __instance, int _index, bool _forceRefresh = false)
        {
            try
            {
                if (!(__instance is CustomCraftingMenu customMenu))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPrefix)}(): Menu is not a {nameof(CustomCraftingMenu)}..");
                    return true;
                }
                Logger.LogDebug($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
                    $"{nameof(CustomCraftingMenu.TryOnRecipeSelected)}({_index}, {_forceRefresh})");
                return !customMenu.TryOnRecipeSelected(_index, _forceRefresh);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPrefix)}(): Exception Invoking {nameof(CustomCraftingMenu.TryOnRecipeSelected)}().", ex);
            }
            return true;
        }

        [HarmonyPatch(nameof(CraftingMenu.OnRecipeSelected), MethodType.Normal)]
        [HarmonyPostfix]
        private static void OnRecipeSelectedPostfix(CraftingMenu __instance, int _index, bool _forceRefresh = false)
        {
            try
            {
                var menuType = __instance?.GetType();
                if (!(__instance is CustomCraftingMenu customMenu))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPostfix)}(): Menu {menuType} is not a {nameof(CustomCraftingMenu)}.");
                    return;
                }
                Logger.LogDebug($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPostfix)}(): Invoked on CraftingMenu type {menuType}. Invoking " +
                    $"{nameof(CustomCraftingMenu.SetSelectorsNavigation)}() and {nameof(CustomCraftingMenu.RefreshResult)}()");
                customMenu.SetSelectorsNavigation(_index);
                customMenu.RefreshResult();
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(OnRecipeSelectedPostfix)}(): Exception Invoking " +
                    $"{nameof(CustomCraftingMenu.SetSelectorsNavigation)}() or {nameof(CustomCraftingMenu.RefreshResult)}().", ex);
            }
        }

        public static event Action<CustomCraftingMenu> RefreshAutoRecipeAfter;
        [HarmonyPatch("RefreshAutoRecipe", MethodType.Normal)]
        [HarmonyPostfix]
        private static void RefreshAutoRecipePostfix(CraftingMenu __instance)
        {
            try
            {
                if (!(__instance is CustomCraftingMenu customMenu))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoRecipePostfix)}(): Menu is not a {nameof(CustomCraftingMenu)}.");
                    return;
                }
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoRecipePostfix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
                    $"{nameof(RefreshAutoRecipeAfter)}()");
                RefreshAutoRecipeAfter?.Invoke(customMenu);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAutoRecipePostfix)}(): Exception Invoking {nameof(RefreshAutoRecipeAfter)}().", ex);
            }
        }

        [HarmonyPatch(nameof(CraftingMenu.OnIngredientSelectorClicked), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool OnIngredientSelectorClickedPrefix(CraftingMenu __instance, int _selectorIndex)
        {
            try
            {
                if (!(__instance is CustomCraftingMenu customMenu))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(OnIngredientSelectorClickedPrefix)}(): Menu is not a {nameof(CustomCraftingMenu)}..");
                    return true;
                }
                Logger.LogDebug($"{nameof(CraftingMenuPatches)}::{nameof(OnIngredientSelectorClickedPrefix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
                    $"{nameof(CustomCraftingMenu.TryOnIngredientSelectorClicked)}({_selectorIndex})");
                return !customMenu.TryOnIngredientSelectorClicked(_selectorIndex);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(OnIngredientSelectorClickedPrefix)}(): Exception Invoking {nameof(CustomCraftingMenu.TryOnIngredientSelectorClicked)}().", ex);
            }
            return true;
        }

        [HarmonyPatch(nameof(CraftingMenu.IngredientSelectorHasChanged), MethodType.Normal)]
        [HarmonyPostfix]
        private static void IngredientSelectorHasChangedPostfix(CraftingMenu __instance, int _selectorIndex, int _itemID)
        {
            try
            {
                if (!(__instance is CustomCraftingMenu customMenu))
                {
                    Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(IngredientSelectorHasChangedPostfix)}(): Menu is not a {nameof(CustomCraftingMenu)}.");
                    return;
                }
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(IngredientSelectorHasChangedPostfix)}(): Invoked on CraftingMenu type {__instance?.GetType()}. Invoking " +
                    $"{nameof(CustomCraftingMenu.IngredientSelectorHasChanged)}({_selectorIndex}, {_itemID})");
                customMenu.IngredientSelectorHasChanged(_selectorIndex, _itemID);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(IngredientSelectorHasChangedPostfix)}(): Exception Invoking {nameof(CustomCraftingMenu.IngredientSelectorHasChanged)}().", ex);
            }
        }

        public static event Action<CustomCraftingMenu> RefreshAvailableIngredientsAfter;
        [HarmonyPatch("RefreshAvailableIngredients", MethodType.Normal)]
        [HarmonyPostfix]
        private static void RefreshAvailableIngredientsPostfix(CraftingMenu __instance)
        {
            try
            {
                if (!(__instance is CustomCraftingMenu customCraftingMenu))
                    return;
                
                Logger.LogTrace($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAvailableIngredientsPostfix)}(): Invoked on CraftingMenu type {customCraftingMenu?.GetType()}.  Invoking {nameof(RefreshAvailableIngredientsAfter)}().");
                RefreshAvailableIngredientsAfter?.Invoke(customCraftingMenu);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuPatches)}::{nameof(RefreshAvailableIngredientsPostfix)}(): Exception Invoking {nameof(RefreshAvailableIngredientsAfter)}({__instance?.GetType()}).", ex);
            }
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

    }
}
