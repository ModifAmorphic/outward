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

        public delegate bool MatchRecipeStep(RecipeIngredient ingredient, out bool isMatch);

        public static event MatchRecipeStep MatchRecipeStepOverride;
        [HarmonyPatch(nameof(CompatibleIngredient.MatchRecipeStep), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool MatchRecipeStepPrefix(CompatibleIngredient __instance, RecipeIngredient _recipeStep, ref bool __result)
        {
            try
            {
                if (!(__instance is CustomCompatibleIngredient customIngredient))
                    return true;
                
                //Logger.LogTrace($"{nameof(CompatibleIngredientPatches)}::{nameof(MatchRecipeStepPrefix)}(): Invoked for RecipeIngredient {_recipeStep}. Invoking {nameof(MatchRecipeStepOverride)}().");
                var origResult = __result;
                if (customIngredient.MatchRecipeStepOverride(_recipeStep, out __result))
                {
                    Logger.LogTrace($"{nameof(CompatibleIngredientPatches)}::{nameof(MatchRecipeStepPrefix)}(): Was overridden by {nameof(MatchRecipeStepOverride)}() and had its result set to {__result}.");
                    return false;
                }
                __result = origResult;
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CompatibleIngredientPatches)}::{nameof(MatchRecipeStepPrefix)}(): Exception Invoking {nameof(MatchRecipeStepOverride)}().", ex);
            }
            Logger.LogTrace($"{nameof(CompatibleIngredientPatches)}::{nameof(MatchRecipeStepPrefix)}(): was not overridden by {nameof(MatchRecipeStepOverride)}(). Allowing base method invocation to continue.");
            return true;
        }
    }
}
