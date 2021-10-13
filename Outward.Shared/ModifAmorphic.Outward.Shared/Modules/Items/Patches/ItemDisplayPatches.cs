using HarmonyLib;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Modules.Items.Patches
{
    [HarmonyPatch(typeof(ItemDisplay))]
    internal static class ItemDisplayPatches
    {
        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<ItemDisplay, Item> RefreshEnchantedIconAfter;

        [HarmonyPatch("RefreshEnchantedIcon", MethodType.Normal)]
        [HarmonyPostfix]
        public static void RefreshEnchantedIconPostfix(ItemDisplay __instance, Item ___m_refItem)
        {
            try
            {
                Logger.LogTrace($"{nameof(ItemDisplayPatches)}::{nameof(RefreshEnchantedIconPostfix)}(): Invoked for ItemDisplay {__instance.name} and Item {___m_refItem?.ItemID} - {___m_refItem?.DisplayName} ({___m_refItem?.UID}). Invoking {nameof(RefreshEnchantedIconAfter)}().");
                RefreshEnchantedIconAfter?.Invoke(__instance, ___m_refItem);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayPatches)}::{nameof(RefreshEnchantedIconPostfix)}(): Exception Invoking {nameof(RefreshEnchantedIconAfter)}().", ex);
            }
        }
    }
}
