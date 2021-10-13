using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Transmorph.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Patches
{
    [HarmonyPatch(typeof(CharacterRecipeKnowledge))]
    internal static class TmogCharacterRecipeKnowledgePatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<CharacterRecipeKnowledge, TransmogRecipe> LearnRecipeBefore;
        [HarmonyPatch(nameof(CharacterRecipeKnowledge.LearnRecipe), MethodType.Normal)]
        [HarmonyPrefix]
        private static void LearnRecipePrefix(CharacterRecipeKnowledge __instance, Recipe _recipe)
        {
            try
            {
                if (_recipe == null || !(_recipe is TransmogRecipe tmogRecipe))
                    return;

                Logger?.LogTrace($"{nameof(TmogCharacterRecipeKnowledgePatches)}::{nameof(LearnRecipePrefix)} called. Invoking {nameof(LearnRecipeBefore)}.");
                LearnRecipeBefore?.Invoke(__instance, tmogRecipe);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(TmogCharacterRecipeKnowledgePatches)}::{nameof(LearnRecipePrefix)}.", ex);
            }
        }
    }
}
