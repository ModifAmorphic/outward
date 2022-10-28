#if DEBUG
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches.Sandbox
{
    [HarmonyPatch(typeof(Character))]
    internal static class CharacterPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);


        public static event Action<Character> AfterProcessInit;

        [HarmonyPatch(nameof(Character.ApplyQuicklots), MethodType.Normal)]
        [HarmonyPostfix]
        private static void ProcessInitPostFix(Character __instance)
        {
            try
            {
                Logger.LogDebug($"{nameof(CharacterPatches)}::{nameof(ProcessInitPostFix)}(): Invoking {nameof(AfterProcessInit)}.");
                AfterProcessInit?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterPatches)}::{nameof(ProcessInitPostFix)}(): Exception invoking {nameof(AfterProcessInit)}.", ex);
            }
        }

        //public delegate void InventoryIngredientsDelegate(CharacterInventory characterInventory, Character character, Tag craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> sortedIngredients);
        //public static event Action<Character> AfterApplyQuicklots;

        //[HarmonyPatch(nameof(Character.ApplyQuicklots), MethodType.Normal)]
        //[HarmonyPatch(new Type[] { typeof(PlayerSaveData) })]
        //[HarmonyPostfix]
        //private static void ApplyQuicklotsPostFix(Character __instance, PlayerSaveData _save)
        //{
        //    try
        //    {
        //        Logger.LogDebug($"{nameof(CharacterPatches)}::{nameof(ApplyQuicklotsPostFix)}(): Invoking {nameof(AfterApplyQuicklots)}.");
        //        AfterApplyQuicklots?.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(CharacterPatches)}::{nameof(ApplyQuicklotsPostFix)}(): Exception invoking {nameof(AfterApplyQuicklots)}.", ex);
        //    }
        //}
    }
}
#endif