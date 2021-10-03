using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Items
{
    [HarmonyPatch(typeof(VisualSlot))]
    public static class VisualSlotPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<VisualSlot, Item> PositionVisualsBefore;
        //[HarmonyPatch(nameof(VisualSlot.PositionVisuals), MethodType.Normal)]
        //[HarmonyPatch(new Type[] { typeof(Item) })]
        //[HarmonyPrefix]
        private static void PositionVisualsPrefix(VisualSlot __instance, Item _item)
        {
            try
            {
                Logger.LogTrace($"{nameof(VisualSlotPatches)}::{nameof(PositionVisualsPrefix)}(): Invoking {nameof(PositionVisualsBefore)}() for item " +
                    $"{_item?.DisplayName} ({_item?.UID}).\n" +
                    $"\t_item.LoadedVisual: {_item.LoadedVisual}\n" +
                    $"\t_item.CurrentVisual: {_item.CurrentVisual}\n" +
                    $"\t_item.EquippedVisuals: {(_item as Equipment)?.EquippedVisuals}\n" +
                    $"\t_item.HasSpecialVisualPrefab: {_item.HasSpecialVisualPrefab}, _item.HasSpecialVisualFemalePrefab: {_item.HasSpecialVisualFemalePrefab}\n" +
                    $"\t_item.UseSpecialVisualFemale: {_item.UseSpecialVisualFemale}, _item.SpecialVisuals: {(_item as Equipment )?.SpecialVisuals}");
                PositionVisualsBefore?.Invoke(__instance, _item);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(VisualSlotPatches)}::{nameof(PositionVisualsPrefix)}(): Exception Invoking {nameof(PositionVisualsBefore)}().", ex);
            }
        }
    }
}
