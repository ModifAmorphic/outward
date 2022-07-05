using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting.Patches
{
    [HarmonyPatch(typeof(CompatibleIngredient))]
    internal static class CompatibleIngredientPatches
    {
        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        [HarmonyPatch(nameof(CompatibleIngredient.MatchRecipeStep), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool MatchRecipeStepPrefix(CompatibleIngredient __instance, RecipeIngredient _recipeStep, ref bool __result)
        {
            string logMethod = nameof(CustomCompatibleIngredient) + "." + nameof(CustomCompatibleIngredient.MatchRecipeStepOverride);
            try
            {
                if (!(__instance is CustomCompatibleIngredient customIngredient))
                    return true;
                
                //Logger.LogTrace($"{nameof(CompatibleIngredientPatches)}::{nameof(MatchRecipeStepPrefix)}(): Invoked for RecipeIngredient {_recipeStep}. Invoking {nameof(MatchRecipeStepOverride)}().");
                var origResult = __result;
                
                if (customIngredient.MatchRecipeStepOverride(_recipeStep, out __result))
                {
                    Logger.LogTrace($"{nameof(CompatibleIngredientPatches)}::{nameof(MatchRecipeStepPrefix)}(): Was overridden by {logMethod}() and had its result set to {__result}.");
                    return false;
                }
                __result = origResult;
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CompatibleIngredientPatches)}::{nameof(MatchRecipeStepPrefix)}(): Exception Invoking {logMethod}().", ex);
            }
            Logger.LogTrace($"{nameof(CompatibleIngredientPatches)}::{nameof(MatchRecipeStepPrefix)}(): was not overridden by {logMethod}(). Allowing base method invocation to continue.");
            return true;
        }

        [HarmonyPatch(nameof(CompatibleIngredient.GetConsumedItems), MethodType.Normal)]
        [HarmonyPostfix]
        private static void GetConsumedItemsPostfix(CompatibleIngredient __instance, bool _useMultipler, ref int _resultMultiplier, ref IList<KeyValuePair<string, int>>  __result)
        {
            string logMethod = nameof(CustomCompatibleIngredient) + "." + nameof(CustomCompatibleIngredient.CaptureConsumedItems);
            try
            {
                if (!(__instance is CustomCompatibleIngredient customIngredient))
                    return;

                Logger.LogTrace($"{nameof(CompatibleIngredientPatches)}::{nameof(GetConsumedItemsPostfix)}(): Capturing {__result?.Count} consumed items.");
                var resultMulti = _resultMultiplier;
                List<Item> preservedItems;
                if (customIngredient.TryGetConsumedItems(_useMultipler, ref resultMulti, out var consumedIngredients, out preservedItems))
                {
                    _resultMultiplier = resultMulti;
                    __result = consumedIngredients;
                }
                customIngredient.CaptureConsumedItems(__result, preservedItems);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CompatibleIngredientPatches)}::{nameof(GetConsumedItemsPostfix)}(): Exception Invoking {logMethod}().", ex);
            }
        }
    }
}
