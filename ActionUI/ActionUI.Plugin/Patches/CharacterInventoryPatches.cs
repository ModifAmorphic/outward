using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(CharacterInventory))]
    internal static class CharacterInventoryPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void InventoryIngredientsDelegate(CharacterInventory characterInventory, Character character, Tag craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> sortedIngredients);
        public static event InventoryIngredientsDelegate AfterInventoryIngredients;

        [HarmonyPatch(nameof(CharacterInventory.InventoryIngredients), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(Tag), typeof(DictionaryExt<int, CompatibleIngredient>) },
            new[] { ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPostfix]
        private static void InventoryIngredientsPostFix(CharacterInventory __instance, Character ___m_character, Tag _craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> _sortedIngredient)
        {
            try
            {
                Logger.LogTrace($"{nameof(CharacterInventoryPatches)}::{nameof(InventoryIngredientsPostFix)}(): Invoking {nameof(AfterInventoryIngredients)}.");
                AfterInventoryIngredients?.Invoke(__instance, ___m_character, _craftingStationTag, ref _sortedIngredient);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterInventoryPatches)}::{nameof(InventoryIngredientsPostFix)}(): Exception invoking {nameof(AfterInventoryIngredients)}.", ex);
            }
        }
    }
}
