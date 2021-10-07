using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Items
{
    [HarmonyPatch(typeof(SingleItemContainerVisual))]
    public static class SingleItemContainerVisualPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();
        
        public delegate bool PreProcessApplyVisualModifications(Item containedItem, out (string UID, int VisualItemID) visualMap);
        public static event PreProcessApplyVisualModifications ApplyVisualModificationsBefore;
        [HarmonyPatch(nameof(SingleItemContainerVisual.ApplyVisualModifications), MethodType.Normal)]
        [HarmonyPrefix]
        public static void ApplyVisualModificationsPrefix(SingleItemContainerVisual __instance, Item ___m_item, out (string UID, int VisualItemID) __state)
        {
            __state = default;
            try
            {
                if (__instance.ContainedItemVisualSlot == null || !(___m_item is SingleItemContainer singleItemContainer)
                    || singleItemContainer.ContainedItem == null || string.IsNullOrEmpty(singleItemContainer.ContainedItem.UID))
                {
                    return;
                }

                Logger.LogTrace($"{nameof(SingleItemContainerVisualPatches)}::{nameof(ApplyVisualModificationsPrefix)}(): Invoked for Item " +
                    $"{singleItemContainer.ContainedItem.ItemID} - {singleItemContainer.ContainedItem.DisplayName} ({singleItemContainer.ContainedItem.UID}). " +
                    $"Invoking {nameof(ApplyVisualModificationsBefore)}().");

                if ((ApplyVisualModificationsBefore?.Invoke(singleItemContainer.ContainedItem, out __state) ?? false))
                {
                    Logger.LogTrace($"{nameof(SingleItemContainerVisualPatches)}::{nameof(ApplyVisualModificationsPrefix)}(): {nameof(ApplyVisualModificationsBefore)}() result: was true. Passing visualMap on to postfix.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SingleItemContainerVisualPatches)}::{nameof(ApplyVisualModificationsPrefix)}(): Exception Invoking {nameof(ApplyVisualModificationsBefore)}().", ex);
            }
            Logger.LogTrace($"{nameof(SingleItemContainerVisualPatches)}::{nameof(ApplyVisualModificationsPrefix)}(): {nameof(ApplyVisualModificationsBefore)}() result: was false. visualMap to postfix will be null");
        }

        public static event Action<(string UID, int VisualItemID)> ApplyVisualModificationsAfter;
        [HarmonyPatch(nameof(SingleItemContainerVisual.ApplyVisualModifications), MethodType.Normal)]
        [HarmonyPrefix]
        public static void ApplyVisualModificationsPostfix(SingleItemContainerVisual __instance, Item ___m_item, (string UID, int VisualItemID) __state)
        {
            try
            {
                if (__state == default)
                    return;

                Logger.LogTrace($"{nameof(SingleItemContainerVisualPatches)}::{nameof(ApplyVisualModificationsPostfix)}(): Invoked for visual map " +
                    $"(UID: '{__state.UID}', VisualItemID: {__state.VisualItemID})" +
                    $"Invoking {nameof(ApplyVisualModificationsAfter)}().");

                ApplyVisualModificationsAfter?.Invoke(__state);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(SingleItemContainerVisualPatches)}::{nameof(ApplyVisualModificationsPostfix)}(): Exception Invoking {nameof(ApplyVisualModificationsAfter)}().", ex);
            }
        }
    }
}
