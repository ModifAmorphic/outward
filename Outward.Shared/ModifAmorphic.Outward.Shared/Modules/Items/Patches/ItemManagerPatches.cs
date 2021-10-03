using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Items
{
    [HarmonyPatch(typeof(ItemManager))]
    public static class ItemManagerPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event GetVisualsByItem GetSpecialVisualsByItemOverride;
        [HarmonyPatch(nameof(ItemManager.GetSpecialVisuals), MethodType.Normal)]
        [HarmonyPrefix]
        [HarmonyPatch(new Type[] { typeof(Item) })]
        public static bool GetSpecialVisualsPrefix(Item _item, ref ItemVisual __result)
        {
            try
            {
                if (_item == null || string.IsNullOrEmpty(_item.UID))
                    return true;

                Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetSpecialVisualsPrefix)}(): Invoked on Item {_item.ItemID} - {_item.DisplayName} ({_item.UID}). Invoking {nameof(GetSpecialVisualsByItemOverride)}().");
                if ((GetSpecialVisualsByItemOverride?.Invoke(_item, out __result) ?? false))
                {
                    Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetSpecialVisualsPrefix)}(): {nameof(GetSpecialVisualsByItemOverride)}() result: was true. Returning false to override base method.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemManagerPatches)}::{nameof(GetSpecialVisualsPrefix)}(): Exception Invoking {nameof(GetSpecialVisualsByItemOverride)}().", ex);
            }
            Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetSpecialVisualsPrefix)}(): {nameof(GetSpecialVisualsByItemOverride)}() result: was false. Returning true and continuing base method invocation.");
            return true;
        }

        public delegate bool GetVisualsByItem(Item input, out ItemVisual output);

        public static event GetVisualsByItem GetVisualsByItemOverride;
        [HarmonyPatch(nameof(ItemManager.GetVisuals), MethodType.Normal)]
        [HarmonyPrefix]
        [HarmonyPatch(new Type[] { typeof(Item) })]
        private static bool GetVisualsByItemPrefix(Item _item, ref ItemVisual __result)
        {
            try
            {
                if (_item == null || string.IsNullOrEmpty(_item.UID))
                    return true;

                Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPrefix)}(): Invoked on Item {_item.ItemID} - {_item.DisplayName} ({_item.UID}). Invoking {nameof(GetVisualsByItemOverride)}()");
                if ((GetVisualsByItemOverride?.Invoke(_item, out __result) ?? false))
                {
                    Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPrefix)}(): {nameof(GetVisualsByItemOverride)}() result: was true. Returning false to override base method.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPrefix)}(): Exception Invoking {nameof(GetVisualsByItemOverride)}().", ex);
            }
            Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPrefix)}(): {nameof(GetVisualsByItemOverride)}() result: was false. Returning true and continuing base method invocation.");
            return true;
        }
    }
}
