using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Patches
{
    [HarmonyPatch(typeof(CharacterRecipeKnowledge))]
    internal static class EnchantCharacterRecipeKnowledgePatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<CharacterRecipeKnowledge, EnchantRecipe> LearnRecipeBefore;
        [HarmonyPatch(nameof(CharacterRecipeKnowledge.LearnRecipe), MethodType.Normal)]
        [HarmonyPrefix]
        private static void LearnRecipePrefix(CharacterRecipeKnowledge __instance, Recipe _recipe)
        {
            try
            {
                if (_recipe == null || !(_recipe is EnchantRecipe encRecipe))
                    return;

                Logger?.LogTrace($"{nameof(EnchantCharacterRecipeKnowledgePatches)}::{nameof(LearnRecipePrefix)} called. Invoking {nameof(LearnRecipeBefore)}.");
                LearnRecipeBefore?.Invoke(__instance, encRecipe);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(EnchantCharacterRecipeKnowledgePatches)}::{nameof(LearnRecipePrefix)}.", ex);
            }
        }
    }
}
