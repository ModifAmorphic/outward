using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Modules.Items.Patches
{
    [HarmonyPatch(typeof(ItemManager))]
    public static class ItemManagerPatches
    {
        //public delegate bool GetVisualsByItem(Item input, out ItemVisual output);
        public delegate void GetVisualsByItem(Item input, ref ItemVisual itemVisual);

        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event GetVisualsByItem GetSpecialVisualsByItemAfter;
        [HarmonyPatch(nameof(ItemManager.GetSpecialVisuals), MethodType.Normal)]
        [HarmonyPostfix]
        [HarmonyPatch(new Type[] { typeof(Item) })]
        public static void GetSpecialVisualsPostfix(ref Item _item, ref ItemVisual __result)
        {
            try
            {
                if (_item == null || string.IsNullOrEmpty(_item.UID))
                    return;

                Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetSpecialVisualsPostfix)}(): Invoked on Item {_item.ItemID} - {_item.DisplayName} ({_item.UID}). Invoking {nameof(GetSpecialVisualsByItemAfter)}().");
                GetSpecialVisualsByItemAfter?.Invoke(_item, ref __result);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemManagerPatches)}::{nameof(GetSpecialVisualsPostfix)}(): Exception Invoking {nameof(GetSpecialVisualsByItemAfter)}().", ex);
            }
        }


        public static event GetVisualsByItem GetVisualsByItemAfter;
        [HarmonyPatch(nameof(ItemManager.GetVisuals), MethodType.Normal)]
        [HarmonyPostfix]
        [HarmonyPatch(new Type[] { typeof(Item) })]
        private static void GetVisualsByItemPostfix(ref Item _item, ref ItemVisual __result)
        {
            try
            {
                if (_item == null || string.IsNullOrEmpty(_item.UID))
                    return;

                Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPostfix)}(): Invoked on Item {_item.ItemID} - {_item.DisplayName} ({_item.UID}). Invoking {nameof(GetVisualsByItemPostfix)}()");
                GetVisualsByItemAfter?.Invoke(_item, ref __result);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemManagerPatches)}::{nameof(GetVisualsByItemPostfix)}(): Exception Invoking {nameof(GetVisualsByItemPostfix)}().", ex);
            }
        }

        [HarmonyPatch(nameof(ItemManager.PutBackVisual), MethodType.Normal)]
        [HarmonyPrefix]
        [HarmonyPatch(new Type[] { typeof(int), typeof(ItemVisual) })]
        public static void PutBackVisualItemIDFix(ref int _itemID, ref ItemVisual _visuals)
        {
            try
            {
                if (_visuals == null)
                    return;

                int nameItemID = _visuals.GetItemIDFromName();
                Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(PutBackVisualItemIDFix)}(): Invoked for ItemID {_itemID} and visual ItemVisual: {_visuals}. GetItemIDFromName(): {nameItemID}.");
                if (nameItemID != _itemID)
                {
                    _itemID = nameItemID;
                    Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(PutBackVisualItemIDFix)}(): ItemID set to {_itemID}.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemManagerPatches)}::{nameof(PutBackVisualItemIDFix)}(): Exception Fixing ItemID for _itemID {_itemID}, _visuals {_visuals}.", ex);
            }
        }
    }
}
