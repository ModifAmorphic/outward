﻿using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(Equipment))]
    internal static class EquipmentPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void OnEquipDelegate(Character character, Equipment equipment);
        public static event OnEquipDelegate AfterOnEquip;

        [HarmonyPatch("OnEquip")]
        [HarmonyPatch(new Type[] { typeof(Character) })]
        [HarmonyPostfix]
        private static void OnEquipPostfix(Equipment __instance, Character _char)
        {
            try
            {
                if (_char == null || _char.OwnerPlayerSys == null || !_char.IsLocalPlayer)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(EquipmentPatches)}::{nameof(OnEquipPostfix)}(): Invoked. Equipped item '{__instance.name}' to slot '{__instance.CurrentEquipmentSlot?.SlotType}' for character {_char?.name}. Invoking {nameof(AfterOnEquip)}.");
#endif
                AfterOnEquip?.Invoke(_char, __instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(EquipmentPatches)}::{nameof(OnEquipPostfix)}(): Exception Invoking {nameof(AfterOnEquip)} for equipment item '{__instance?.name}' and character {_char?.name}.", ex);
            }
        }

        public delegate void OnUnequipDelegate(Character character, Equipment equipment);
        public static event OnUnequipDelegate AfterOnUnequip;

        [HarmonyPatch("OnUnequip")]
        [HarmonyPatch(new Type[] { typeof(Character) })]
        [HarmonyPostfix]
        private static void OnUnequipPostfix(Equipment __instance, Character _char)
        {
            try
            {
                if (_char == null || _char.OwnerPlayerSys == null || !_char.IsLocalPlayer)
                    return;
#if DEBUG
                Logger.LogTrace($"{nameof(EquipmentPatches)}::{nameof(OnUnequipPostfix)}(): Invoked. Unequipped item '{__instance.name}' from slot '{__instance.CurrentEquipmentSlot?.SlotType}' for character {_char?.name}. Invoking {nameof(AfterOnUnequip)}.");
#endif
                AfterOnUnequip?.Invoke(_char, __instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(EquipmentPatches)}::{nameof(OnUnequipPostfix)}(): Exception invoking {nameof(AfterOnUnequip)} for character {_char?.name} and equipment {__instance?.name}.", ex);
            }
        }
    }
}
