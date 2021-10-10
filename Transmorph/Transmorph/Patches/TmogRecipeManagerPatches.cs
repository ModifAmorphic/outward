using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Patches
{
    [HarmonyPatch(typeof(RecipeManager))]
    internal static class TmogRecipeManagerPatches
    {
        public static event Action<RecipeManager> LoadCraftingRecipeAfter;
        [HarmonyPatch("LoadCraftingRecipe", MethodType.Normal)]
        [HarmonyPostfix]
        private static void LoadCraftingRecipePostfix(RecipeManager __instance)
        {
            LoadCraftingRecipeAfter?.Invoke(__instance);
        }
    }
}
