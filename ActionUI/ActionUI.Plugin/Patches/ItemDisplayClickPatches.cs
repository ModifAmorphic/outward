using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using Rewired;
using UnityEngine;
using System;
using System.Collections.Generic;
using ModifAmorphic.Outward.Extensions;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(ItemDisplayClick))]
    internal static class ItemDisplayClickPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate bool TryOverrideOnItemCtrlClickedDelegate(ItemDisplay itemDisplay, bool stashKeyPressed);
        public static Dictionary<int, TryOverrideOnItemCtrlClickedDelegate> PlayersOnItemCtrlClicked = new Dictionary<int, TryOverrideOnItemCtrlClickedDelegate>();

        [HarmonyPatch("DoubleClick")]
        [HarmonyPatch(new Type[] {  })]
        [HarmonyPrefix]
        private static bool DoubleClickPrefix(ItemDisplayClick __instance, ItemDisplay ___m_itemDisplay)
        {
            try
            {
                
//#if DEBUG
//                Logger.LogTrace($"{nameof(ItemDisplayClickPatches)}::{nameof(DoubleClickPrefix)}(): ItemGroupDisplay {__instance.name}, player ID {__instance.PlayerID}, " +
//                    $"Item {__instance.RefItem?.name}, ControllerType {player.controllers.GetLastActiveController().type}, " +
//                    $"Control Pressed {Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)}, " +
//                    $"Left Click {Input.GetKey(KeyCode.Mouse0)}, " +
//                    $"Any Key Down {Input.anyKey}, " +
//                    $"PlayersOnItemCtrlClicked.ContainsKey({__instance.PlayerID}) {PlayersOnItemCtrlClicked.ContainsKey(__instance.PlayerID)}");
//#endif
                if (___m_itemDisplay == null || ___m_itemDisplay.RefItem == null)
                    return true;
                
                if (___m_itemDisplay.RefItem is Skill || !PlayersOnItemCtrlClicked.TryGetValue(___m_itemDisplay.PlayerID, out var overrideOnClicked))
                    return true;
                
                var player = ReInput.players.GetPlayer(___m_itemDisplay.PlayerID);
                //If player is not using a keyboard and mouse, exit
                if (!(player.controllers.GetLastActiveController().type == ControllerType.Keyboard || player.controllers.GetLastActiveController().type == ControllerType.Mouse))
                    return true;

                var ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#if DEBUG
                Logger.LogTrace($"{nameof(ItemDisplayClickPatches)}::{nameof(DoubleClickPrefix)}(): Invoking {nameof(PlayersOnItemCtrlClicked)} for ItemDisplayClick {__instance.name} and player ID {___m_itemDisplay.PlayerID}.");
#endif
                if (overrideOnClicked.Invoke(___m_itemDisplay, ctrlPressed))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayClickPatches)}::{nameof(DoubleClickPrefix)}(): Exception invoking {nameof(PlayersOnItemCtrlClicked)} for ItemDisplayClick {__instance.name}.", ex);
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
