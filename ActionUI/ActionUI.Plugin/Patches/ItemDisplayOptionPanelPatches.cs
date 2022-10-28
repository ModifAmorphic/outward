using HarmonyLib;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Localization;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(ItemDisplayOptionPanel))]
    internal static class ItemDisplayOptionPanelPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static int PlayersMoveToStashID = -1;
        [HarmonyPatch("StartInit")]
        [HarmonyPatch(new Type[] { })]
        [HarmonyPostfix]
        private static void AddStashActionString(ItemDisplayOptionPanel __instance, ref string[] ___ActionStrings)
        {
            try
            { 
            //{
            //    if (__instance.LocalCharacter?.OwnerPlayerSys == null)
            //        return;

                var indexOf = ___ActionStrings.IndexOf(InventorySettings.MoveToStashKey);
                //var playerId = __instance.LocalCharacter.OwnerPlayerSys.PlayerID;

#if DEBUG
                Logger.LogTrace($"{nameof(ItemDisplayOptionPanelPatches)}::{nameof(AddStashActionString)}(): Adding new context action string {InventorySettings.MoveToStashKey}. indexOf == {indexOf}");
#endif

                if (indexOf != -1)
                {
                    PlayersMoveToStashID =  indexOf;
                    return;
                }
                var actionStringsList = ___ActionStrings.ToList();
                actionStringsList.Add(InventorySettings.MoveToStashKey);
                ___ActionStrings = actionStringsList.ToArray();
                PlayersMoveToStashID = ___ActionStrings.Length - 1;

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayOptionPanelPatches)}::{nameof(AddStashActionString)}(): Exception adding new action {InventorySettings.MoveToStashKey}.", ex);
            }
        }

        public delegate bool TryGetActiveActionsDelegate(int rewiredId, Item item, List<int> baseActiveActions, out List<int> activeActions);
        public static Dictionary<int, TryGetActiveActionsDelegate> TryGetActiveActions = new Dictionary<int, TryGetActiveActionsDelegate>();
        [HarmonyPatch("GetActiveActions")]
        [HarmonyPatch(new Type[] { typeof(GameObject) })]
        [HarmonyPostfix]
        private static void GetActiveActionsPostfix(ItemDisplayOptionPanel __instance, Item ___m_pendingItem, GameObject pointerPress, ref List<int> __result)
        {
            try
            {
                if (___m_pendingItem == null)
                    return;

                if (__instance.LocalCharacter?.OwnerPlayerSys == null)
                    return;

                var playerId = __instance.LocalCharacter.OwnerPlayerSys.PlayerID;

                if (TryGetActiveActions.TryGetValue(playerId, out var tryGetActiveActions))
                {
#if DEBUG
                    //Logger.LogTrace($"{nameof(ItemDisplayOptionPanelPatches)}::{nameof(GetActiveActionsPostfix)}(): Attempting to get ActiveActions from {TryGetActiveActions}.");
#endif
                    if (tryGetActiveActions(playerId, ___m_pendingItem, __result, out var activeActions))
                        __result = activeActions;
                }

            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayOptionPanelPatches)}::{nameof(GetActiveActionsPostfix)}(): Exception Invoking {nameof(TryGetActiveActions)}.", ex);
            }
        }

        public delegate void PressedActionDelegate(int actionID, ItemDisplay itemDisplay);
        public static Dictionary<int, PressedActionDelegate> PlayersPressedAction = new Dictionary<int, PressedActionDelegate>();
        [HarmonyPatch(nameof(ItemDisplayOptionPanel.ActionHasBeenPressed))]
        [HarmonyPatch(new Type[] { typeof(int) })]
        [HarmonyPrefix]
        private static void ActionHasBeenPressedPrefix(ItemDisplayOptionPanel __instance, ItemDisplay ___m_activatedItemDisplay, int _actionID)
        {
            try
            {
                if (__instance.LocalCharacter?.OwnerPlayerSys == null || __instance.LocalCharacter?.Inventory == null)
                    return;

                var playerId = __instance.LocalCharacter.OwnerPlayerSys.PlayerID;

                if (PlayersPressedAction.TryGetValue(playerId, out var pressedAction))
                {
#if DEBUG
                    Logger.LogTrace($"{nameof(ItemDisplayOptionPanelPatches)}::{nameof(ActionHasBeenPressedPrefix)}(): Invoking {PlayersPressedAction} for for player {playerId}.");
#endif
                    pressedAction(_actionID, ___m_activatedItemDisplay);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemDisplayOptionPanelPatches)}::{nameof(ActionHasBeenPressedPrefix)}(): Exception Invoking {nameof(PlayersPressedAction)} for PlayerID {__instance.LocalCharacter?.OwnerPlayerSys?.PlayerID}.", ex);
            }
        }

        //        public delegate void ShowMenuDelegate(CharacterUI characterUI, CharacterUI.MenuScreens menu, Item item);
        //        public static event ShowMenuDelegate BeforeShowMenu;
        //        [HarmonyPatch(nameof(CharacterUI.ShowMenu))]
        //        [HarmonyPatch(new Type[] { typeof(CharacterUI.MenuScreens), typeof(Item) })]
        //        [HarmonyPrefix]
        //        private static void ShowMenuPrefix(CharacterUI __instance, CharacterUI.MenuScreens _menu, Item _item)
        //        {
        //            try
        //            {
        //                if (__instance.TargetCharacter == null || __instance.TargetCharacter.OwnerPlayerSys == null || !__instance.TargetCharacter.IsLocalPlayer)
        //                    return;

        //#if DEBUG
        //                Logger.LogTrace($"{nameof(ItemDisplayOptionPanelPatches)}::{nameof(ShowMenuPrefix)}(): Invoking {nameof(BeforeShowMenu)} for character {__instance.TargetCharacter.UID}.");
        //#endif
        //                BeforeShowMenu?.Invoke(__instance, _menu, _item);

        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.LogException($"{nameof(ItemDisplayOptionPanelPatches)}::{nameof(ShowMenuPrefix)}(): Exception Invoking {nameof(BeforeShowMenu)} for character {__instance.TargetCharacter?.UID}.", ex);
        //            }
        //        }
    }
}
