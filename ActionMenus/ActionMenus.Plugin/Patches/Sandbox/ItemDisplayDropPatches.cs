using HarmonyLib;
using ModifAmorphic.Outward.ActionMenus.Services;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Logging;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;

namespace ModifAmorphic.Outward.ActionMenus.Patches
{
    [HarmonyPatch(typeof(ItemDisplayDrop))]
    internal static class ItemDisplayDropPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<ItemDisplayDrop, PointerEventData, ItemDisplay> AfterGetDraggedElement;

        [HarmonyPatch("GetDraggedElement")]
        [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
        [HarmonyPostfix]
        private static void GetDraggedElementPostfix(ItemDisplayDrop __instance, PointerEventData _data, ItemDisplay __result)
        {
            try
            {
                Logger.LogTrace($"{nameof(ItemDisplayDropPatches)}::{nameof(GetDraggedElementPostfix)}(): Invoked. Invoking {nameof(AfterGetDraggedElement)}. ItemDisplay: {__result?.name}, PointerEventData.pointerDrag: {_data.pointerDrag?.name}.");
                AfterGetDraggedElement?.Invoke(__instance, _data, __result);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayDropPatches)}::{nameof(GetDraggedElementPostfix)}(): Exception invoking {nameof(AfterGetDraggedElement)}.", ex);
            }
        }
    }
}
