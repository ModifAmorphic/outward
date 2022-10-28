using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using Rewired;
using UnityEngine;
using System;
using System.Collections.Generic;
using ModifAmorphic.Outward.Extensions;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(CurrencyDisplayClick))]
    internal static class CurrencyDisplayClickPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate bool TryOverrideOnCurrencyCtrlClickedDelegate(CurrencyDisplay currencyDisplay, bool stashKeyPressed);
        public static Dictionary<int, TryOverrideOnCurrencyCtrlClickedDelegate> PlayersOnCurrencyCtrlClicked = new Dictionary<int, TryOverrideOnCurrencyCtrlClickedDelegate>();

        [HarmonyPatch("DoubleClick")]
        [HarmonyPatch(new Type[] {  })]
        [HarmonyPrefix]
        private static bool DoubleClickPrefix(CurrencyDisplayClick __instance, CurrencyDisplay ___m_currencyDisplay)
        {
            try
            {
                if (___m_currencyDisplay?.CurrentCurrencyHolder == null)
                    return true;

                if (!PlayersOnCurrencyCtrlClicked.TryGetValue(___m_currencyDisplay.PlayerID, out var overrideOnClicked))
                    return true;

                var player = ReInput.players.GetPlayer(___m_currencyDisplay.PlayerID);
                //If player is not using a keyboard and mouse, exit
                if (!(player.controllers.GetLastActiveController().type == ControllerType.Keyboard || player.controllers.GetLastActiveController().type == ControllerType.Mouse))
                    return true;

                var ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

#if DEBUG
                Logger.LogTrace($"{nameof(CurrencyDisplayClickPatches)}::{nameof(DoubleClickPrefix)}(): Invoking {nameof(PlayersOnCurrencyCtrlClicked)} for CurrencyDisplayClick {__instance.name} and player ID {___m_currencyDisplay.PlayerID}.");
#endif
                if (overrideOnClicked.Invoke(___m_currencyDisplay, ctrlPressed))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CurrencyDisplayClickPatches)}::{nameof(DoubleClickPrefix)}(): Exception invoking {nameof(PlayersOnCurrencyCtrlClicked)} for CurrencyDisplayClick {__instance.name}.", ex);
            }
            return true;
        }

        //public delegate void AfterRefreshEnchantedIconDelegate(ItemGroupDisplay itemDisplay, EquipmentSetSkill setSkill);
        //public static event AfterRefreshEnchantedIconDelegate AfterRefreshEnchantedIcon;

        //[HarmonyPatch("RefreshEnchantedIcon", MethodType.Normal)]
        //[HarmonyPostfix]
        //public static void RefreshEnchantedIconPostfix(ItemGroupDisplay __instance, Item ___m_refItem)
        //{
        //    try
        //    {
        //        if (___m_refItem == null || !(___m_refItem is EquipmentSetSkill equipSetSkill))
        //            return;

        //        Logger.LogTrace($"{nameof(ItemGroupDisplayPatches)}::{nameof(RefreshEnchantedIconPostfix)}(): Invoked for ItemGroupDisplay {__instance.name} and Item {___m_refItem?.ItemID} - {___m_refItem?.DisplayName} ({___m_refItem?.UID}). Invoking {nameof(AfterRefreshEnchantedIcon)}().");
        //        AfterRefreshEnchantedIcon?.Invoke(__instance, equipSetSkill);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(ItemGroupDisplayPatches)}::{nameof(RefreshEnchantedIconPostfix)}(): Exception Invoking {nameof(AfterRefreshEnchantedIcon)}().", ex);
        //    }
        //}
    }
}
