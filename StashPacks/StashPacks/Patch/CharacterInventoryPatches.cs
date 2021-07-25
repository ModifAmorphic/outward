using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(CharacterInventory))]
    internal static class CharacterInventoryPatches
    {
        [HarmonyPatch(nameof(CharacterInventory.DropItem))]
        [HarmonyPatch(new Type[] { typeof(Item), typeof(Transform), typeof(bool) })]
        [HarmonyPostfix]
        public static void DropItemPostfix(Item _item, Transform _newParent = null, bool _playAnim = true)
        {
            CharacterInventoryEvents.RaiseDropBagItemAfter(_item);
        }
    }
}
