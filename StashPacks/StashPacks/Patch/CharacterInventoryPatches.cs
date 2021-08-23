using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using System;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(CharacterInventory))]
    internal static class CharacterInventoryPatches
    {
        [HarmonyPatch(nameof(CharacterInventory.DropItem))]
        [HarmonyPatch(new Type[] { typeof(Item), typeof(Transform), typeof(bool) })]
        [HarmonyPrefix]
        private static bool DropItemPrefix(Character ___m_character, Item _item, Transform _newParent = null, bool _playAnim = true)
        {
            CharacterInventoryEvents.RaiseDropBagItemBefore(___m_character, ref _item, _newParent, _playAnim);
            return true;
        }

        [HarmonyPatch(nameof(CharacterInventory.DropItem))]
        [HarmonyPatch(new Type[] { typeof(Item), typeof(Transform), typeof(bool) })]
        [HarmonyPostfix]
        private static void DropItemPostfix(Character ___m_character, Item _item, Transform _newParent = null, bool _playAnim = true)
        {
            CharacterInventoryEvents.RaiseDropBagItemAfter(___m_character, _item);
        }

        [HarmonyPatch(nameof(CharacterInventory.GetMostRelevantContainer))]
        [HarmonyPostfix]
        private static void GetMostRelevantContainerPostfix(Item _item, ItemContainer ___m_inventoryPouch, ref ItemContainer __result)
        {
            CharacterInventoryEvents.RaiseGetMostRelevantContainerAfter(_item, ___m_inventoryPouch, ref __result);
        }

        [HarmonyPatch(nameof(CharacterInventory.InventoryIngredients), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(Tag), typeof(DictionaryExt<int, CompatibleIngredient>) },
            new[] { ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPostfix]
        private static void InventoryIngredientsPostFix(CharacterInventory __instance, Character ___m_character, Tag _craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> _sortedIngredient)
        {
            CharacterInventoryEvents.RaiseInventoryIngredientsAfter(__instance, ___m_character, _craftingStationTag, ref _sortedIngredient);
        }
    }
}
