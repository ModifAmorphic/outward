using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(InventoryContentDisplay))]
    internal static class InventoryContentDisplayPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        //public static event Action<InventoryContentDisplay> BeforeShow;

        //[HarmonyPatch(nameof(InventoryContentDisplay.Show))]
        //[HarmonyPrefix]
        //private static void ShowPrefix(InventoryContentDisplay __instance)
        //{
        //    try
        //    {
        //        if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
        //            return;

        //        Logger.LogDebug($"{nameof(InventoryContentDisplay)}::{nameof(ShowPrefix)}(): Invoking {nameof(BeforeShow)}.");
        //        BeforeShow?.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(InventoryContentDisplay)}::{nameof(ShowPrefix)}(): Exception invoking {nameof(BeforeShow)}.", ex);
        //    }
        //}

        //public static event Action<InventoryContentDisplay> AfterDelayedFocus;

        //[HarmonyPatch("DelayedFocus")]
        //[HarmonyPostfix]
        //private static void DelayedFocusPostfix(InventoryContentDisplay __instance)
        //{
        //    try
        //    {
        //        if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
        //            return;

        //        Logger.LogDebug($"{nameof(InventoryContentDisplay)}::{nameof(DelayedFocusPostfix)}(): Invoking {nameof(AfterDelayedFocus)}.");
        //        AfterDelayedFocus?.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(InventoryContentDisplay)}::{nameof(DelayedFocusPostfix)}(): Exception invoking {nameof(AfterDelayedFocus)}.", ex);
        //    }
        //}

        //public static event Action<InventoryContentDisplay> AfterStartInit;

        //[HarmonyPatch("StartInit")]
        //[HarmonyPostfix]
        //private static void StartInitPostfix(InventoryContentDisplay __instance)
        //{
        //    try
        //    {
        //        if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
        //            return;

        //        Logger.LogDebug($"{nameof(InventoryContentDisplay)}::{nameof(StartInitPostfix)}(): Invoking {nameof(AfterStartInit)}.");
        //        AfterStartInit?.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(InventoryContentDisplay)}::{nameof(StartInitPostfix)}(): Exception invoking {nameof(AfterStartInit)}.", ex);
        //    }
        //}

        public static event Action<InventoryContentDisplay> AfterOnHide;

        [HarmonyPatch("OnHide")]
        [HarmonyPostfix]
        private static void OnHidePostfix(InventoryContentDisplay __instance)
        {
            try
            {
                if (__instance.LocalCharacter == null || __instance.LocalCharacter.OwnerPlayerSys == null || !__instance.LocalCharacter.IsLocalPlayer)
                    return;

                Logger.LogDebug($"{nameof(InventoryContentDisplay)}::{nameof(OnHidePostfix)}(): Invoking {nameof(AfterOnHide)}.");
                AfterOnHide?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InventoryContentDisplay)}::{nameof(OnHidePostfix)}(): Exception invoking {nameof(AfterOnHide)}.", ex);
            }
        }

        public delegate void AfterFocusMostRelevantItemDelegate(InventoryContentDisplay inventoryContentDisplay, ItemListDisplay excludedList, bool result);
        public static event AfterFocusMostRelevantItemDelegate AfterFocusMostRelevantItem;

        [HarmonyPatch("FocusMostRelevantItem")]
        [HarmonyPatch(new Type[] { typeof(ItemListDisplay) })]
        [HarmonyPostfix]
        private static void FocusMostRelevantItemPostfix(InventoryContentDisplay __instance, ItemListDisplay _excludedList, bool __result)
        {
            try
            {
                Logger.LogTrace($"{nameof(InventoryContentDisplay)}::{nameof(FocusMostRelevantItemPostfix)}(): Invoking {nameof(AfterFocusMostRelevantItem)}.");
                AfterFocusMostRelevantItem?.Invoke(__instance, _excludedList, __result);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InventoryContentDisplay)}::{nameof(FocusMostRelevantItemPostfix)}(): Exception invoking {nameof(AfterFocusMostRelevantItem)}.", ex);
            }
        }

        public delegate void AfterSetContainersVisibilityDelegate(InventoryContentDisplay inventoryContentDisplay, bool showPouch, bool showBag, bool showEquipment);
        public static event AfterSetContainersVisibilityDelegate AfterSetContainersVisibility;

        [HarmonyPatch("SetContainersVisibility")]
        [HarmonyPatch(new Type[] { typeof(bool), typeof(bool), typeof(bool) })]
        [HarmonyPostfix]
        private static void SetContainersVisibilityPostfix(InventoryContentDisplay __instance, bool _showPouch, bool _showBag, bool _showEquipment)
        {
            try
            {
                Logger.LogDebug($"{nameof(InventoryContentDisplay)}::{nameof(SetContainersVisibilityPostfix)}(): Invoking {nameof(AfterSetContainersVisibility)}.");
                AfterSetContainersVisibility?.Invoke(__instance, _showPouch, _showBag, _showEquipment);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InventoryContentDisplay)}::{nameof(SetContainersVisibilityPostfix)}(): Exception invoking {nameof(AfterSetContainersVisibility)}.", ex);
            }
        }

        public delegate void AfterRefreshReferencesDelegate(InventoryContentDisplay inventoryContentDisplay, bool forceRefresh);
        public static event AfterRefreshReferencesDelegate AfterRefreshReferences;

        [HarmonyPatch("RefreshReferences")]
        [HarmonyPatch(new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        private static void RefreshReferencesPostfix(InventoryContentDisplay __instance, bool _forceRefresh)
        {
            try
            {
                Logger.LogDebug($"{nameof(InventoryContentDisplay)}::{nameof(RefreshReferencesPostfix)}(): Invoking {nameof(AfterRefreshReferences)}.");
                AfterRefreshReferences?.Invoke(__instance, _forceRefresh);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InventoryContentDisplay)}::{nameof(RefreshReferencesPostfix)}(): Exception invoking {nameof(AfterRefreshReferences)}.", ex);
            }
        }


        public delegate void AfterRefreshContainerDisplaysDelegate(InventoryContentDisplay inventoryContentDisplay, bool clearAssignedDisplay);
        public static event AfterRefreshContainerDisplaysDelegate AfterRefreshContainerDisplays;

        [HarmonyPatch("RefreshContainerDisplays")]
        [HarmonyPatch(new Type[] { typeof(bool) })]
        [HarmonyPostfix]
        private static void RefreshContainerDisplaysPostfix(InventoryContentDisplay __instance, bool _clearAssignedDisplay)
        {
            try
            {
                Logger.LogDebug($"{nameof(InventoryContentDisplay)}::{nameof(RefreshContainerDisplaysPostfix)}(): Invoking {nameof(AfterRefreshContainerDisplays)}.");
                AfterRefreshContainerDisplays?.Invoke(__instance, _clearAssignedDisplay);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InventoryContentDisplay)}::{nameof(RefreshContainerDisplaysPostfix)}(): Exception invoking {nameof(AfterRefreshContainerDisplays)}.", ex);
            }
        }

    }
}
