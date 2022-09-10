using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Patches
{
    [HarmonyPatch(typeof(RecipeManager))]
    internal static class EnchantRecipeManagerPatches
    {

        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<RecipeManager> LoadEnchantingRecipeAfter;
        [HarmonyPatch("LoadEnchantingRecipe", MethodType.Normal)]
        [HarmonyPostfix]
        private static void LoadEnchantingRecipePostfix(RecipeManager __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(EnchantRecipeManagerPatches)}::{nameof(LoadEnchantingRecipePostfix)}(): Invoking {nameof(LoadEnchantingRecipeAfter)}().");
                LoadEnchantingRecipeAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(EnchantRecipeManagerPatches)}::{nameof(LoadEnchantingRecipePostfix)}(): Exception Invoking {nameof(LoadEnchantingRecipeAfter)}().", ex);
            }
        }
    }
}
