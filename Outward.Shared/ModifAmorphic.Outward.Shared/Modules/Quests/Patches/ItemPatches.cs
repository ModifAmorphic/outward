using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Quests.Patches
{
    [HarmonyPatch(typeof(Item))]
    public static class ItemPatches
    {
        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<Item> GetItemIconBefore;

        [HarmonyPatch(nameof(Item.ItemIcon), MethodType.Getter)]
        [HarmonyPrefix]
        private static void ItemIconPrefix(Item __instance)
        {
#if DEBUG
            //Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(ItemIconPrefix)}: Triggered for Item {__instance.DisplayName} ({__instance.ItemID}).");
#endif

            try
            {
                GetItemIconBefore?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemPatches)}::{nameof(ItemIconPrefix)}(): Exception Invoking {nameof(GetItemIconBefore)}().", ex);
            }
        }
    }
}
