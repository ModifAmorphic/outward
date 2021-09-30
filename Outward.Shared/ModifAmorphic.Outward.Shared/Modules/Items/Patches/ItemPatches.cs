using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Items
{
    [HarmonyPatch(typeof(Item))]
    public static class ItemPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<Item> GetItemIconBefore;

        public static event Func<(Item item, Transform itemVisual, bool special), Transform> GetItemVisualBefore;

        [HarmonyPatch(nameof(Item.ItemIcon), MethodType.Getter)]
        [HarmonyPrefix]
        private static void ItemIconPrefix(Item __instance)
        {
#if DEBUG
            //Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(ItemIconPrefix)}: Triggered for Item {__instance.DisplayName} ({__instance.ItemID}).");
#endif
            GetItemIconBefore?.Invoke(__instance);
        }

        [HarmonyPatch(nameof(Item.GetItemVisual), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        private static bool GetItemVisualPrefix(Item __instance, bool _special, ref Transform __result)
        {
#if DEBUG
            //Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(ItemIconPrefix)}: Triggered for Item {__instance.DisplayName} ({__instance.ItemID}).");
#endif
            var itemVisual = GetItemVisualBefore?.Invoke((__instance, __result, _special));
            if (itemVisual != null)
            {
                __result = itemVisual;
                return false;
            }
            return true;
        }
    }
}
