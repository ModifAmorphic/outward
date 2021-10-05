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
        [PatchLogger]
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
    }
}
