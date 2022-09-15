using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Patches
{
    [HarmonyPatch(typeof(RecipeManager))]
    internal static class TmogRecipeManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<RecipeManager> LoadCraftingRecipeAfter;
        [HarmonyPatch("LoadCraftingRecipe", MethodType.Normal)]
        [HarmonyPostfix]
        private static void LoadCraftingRecipePostfix(RecipeManager __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(TmogRecipeManagerPatches)}::{nameof(LoadCraftingRecipePostfix)}(): Invoking {nameof(LoadCraftingRecipeAfter)}().");
                LoadCraftingRecipeAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(TmogRecipeManagerPatches)}::{nameof(LoadCraftingRecipePostfix)}(): Exception Invoking {nameof(LoadCraftingRecipeAfter)}().", ex);
            }
        }
    }
}
