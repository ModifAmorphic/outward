using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Items.Patches
{
    [HarmonyPatch(typeof(VisualSlot))]
    public static class VisualSlotPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();
        
        public delegate bool SetResultPositionVisuals(VisualSlot visualSlot, ref Item input);
        public static event SetResultPositionVisuals PositionVisualsOverride;
        [HarmonyPatch(nameof(VisualSlot.PositionVisuals), MethodType.Normal)]
        [HarmonyPrefix]
        [HarmonyPatch(new Type[] { typeof(Item) })]
        public static bool PositionVisualsPrefix(VisualSlot __instance, ref Item _item)
        {
            try
            {
                if (_item == null || string.IsNullOrEmpty(_item.UID))
                    return true;

                Logger.LogTrace($"{nameof(VisualSlotPatches)}::{nameof(PositionVisualsPrefix)}(): Invoked on Item {_item.ItemID} - {_item.DisplayName} ({_item.UID}). Invoking {nameof(PositionVisualsPrefix)}().");
                if ((PositionVisualsOverride?.Invoke(__instance, ref _item) ?? false))
                {
                    Logger.LogTrace($"{nameof(VisualSlotPatches)}::{nameof(PositionVisualsPrefix)}(): {nameof(PositionVisualsOverride)}() result: was true. Returning false to override base method.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(VisualSlotPatches)}::{nameof(PositionVisualsPrefix)}(): Exception Invoking {nameof(PositionVisualsOverride)}().", ex);
            }
            Logger.LogTrace($"{nameof(VisualSlotPatches)}::{nameof(PositionVisualsPrefix)}(): {nameof(PositionVisualsOverride)}() result: was false. Returning true and continuing base method invocation.");
            return true;
        }

        

        //[HarmonyPatch(nameof(ItemManager.GetVisuals), MethodType.Normal)]
        //[HarmonyPrefix]
        //[HarmonyPatch(new Type[] { typeof(Item) })]
        //private static bool GetVisualsByItemPrefix(Item _item, ref ItemVisual __result)
        //{
        //    try
        //    {
        //        if (_item == null || string.IsNullOrEmpty(_item.UID))
        //            return true;

        //        Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPrefix)}(): Invoked on Item {_item.ItemID} - {_item.DisplayName} ({_item.UID}). Invoking {nameof(GetVisualsByItemOverride)}()");
        //        if ((GetVisualsByItemOverride?.Invoke(_item, out __result) ?? false))
        //        {
        //            Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPrefix)}(): {nameof(GetVisualsByItemOverride)}() result: was true. Returning false to override base method.");
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPrefix)}(): Exception Invoking {nameof(GetVisualsByItemOverride)}().", ex);
        //    }
        //    Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPrefix)}(): {nameof(GetVisualsByItemOverride)}() result: was false. Returning true and continuing base method invocation.");
        //    return true;
        //}
    }
}
