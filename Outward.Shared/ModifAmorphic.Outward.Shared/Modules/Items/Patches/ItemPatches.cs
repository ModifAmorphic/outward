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

        [HarmonyPatch(nameof(Item.ItemIcon), MethodType.Getter)]
        [HarmonyPrefix]
        public static void ItemIconPrefix(Item __instance)
        {
#if DEBUG
            //Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(ItemIconPrefix)}: Triggered for Item {__instance.DisplayName} ({__instance.ItemID}).");
#endif
            GetItemIconBefore?.Invoke(__instance);
        }
    }
}
