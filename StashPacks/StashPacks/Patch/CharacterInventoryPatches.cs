﻿using HarmonyLib;
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
        public static bool DropItemPrefix(Character ___m_character, Item _item, Transform _newParent = null, bool _playAnim = true)
        {
            CharacterInventoryEvents.RaiseDropBagItemBefore(___m_character, ref _item);
            return true;
        }

        [HarmonyPatch(nameof(CharacterInventory.DropItem))]
        [HarmonyPatch(new Type[] { typeof(Item), typeof(Transform), typeof(bool) })]
        [HarmonyPostfix]
        public static void DropItemPostfix(Character ___m_character, Item _item, Transform _newParent = null, bool _playAnim = true)
        {
            CharacterInventoryEvents.RaiseDropBagItemAfter(___m_character, _item);
        }

        [HarmonyPatch(nameof(CharacterInventory.InventoryIngredients), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(Tag), typeof(DictionaryExt<int, CompatibleIngredient>) },
            new[] { ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPostfix]
        public static void InventoryIngredientsPostFix(CharacterInventory __instance, Character ___m_character, Tag _craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> _sortedIngredient)
        {
            CharacterInventoryEvents.RaiseInventoryIngredientsAfter(__instance, ___m_character, _craftingStationTag, ref _sortedIngredient);
        }
    }
}