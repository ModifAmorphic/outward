using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

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

        public delegate bool OwnsItemDelegate(string itemUID);
        public static Dictionary<int, OwnsItemDelegate> OwnsItemDelegates = new Dictionary<int, OwnsItemDelegate>();

        [HarmonyPatch(nameof(CharacterInventory.OwnsItem), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(string) })]
        [HarmonyPostfix]
        private static void OwnsItemPostFix(CharacterInventory __instance, Character ___m_character, string _itemUID, ref bool __result)
        {
            try
            {
                if (__result == true || ___m_character?.OwnerPlayerSys == null)
                    return;

                var playerId = ___m_character.OwnerPlayerSys.PlayerID;

                if (OwnsItemDelegates.TryGetValue(playerId, out var ownsItem))
                {
                    //Called on Update. Spams logs
                    Logger.LogDebug($"{nameof(CharacterInventoryPatches)}::{nameof(OwnsItemPostFix)}(): Invoked for PlayerID {playerId}. Invoking {nameof(OwnsItemDelegates)} delegate.");

                    __result = ownsItem(_itemUID);
                    if (__result)
                        Logger.LogDebug($"Found item with UID '{_itemUID}' in stash.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterInventoryPatches)}::{nameof(OwnsItemPostFix)}(): Exception invoking {nameof(OwnsItemDelegates)} delegate for PlayerID {___m_character?.OwnerPlayerSys?.PlayerID}.", ex);
            }
        }
    }
}
