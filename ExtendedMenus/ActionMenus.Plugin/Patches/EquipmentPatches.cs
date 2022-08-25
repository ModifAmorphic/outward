using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Patches
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
                if (!_char.IsLocalPlayer)
                    return;

                Logger.LogTrace($"{nameof(EquipmentPatches)}::{nameof(OnEquipPostfix)}(): Invoked. Equipment item '{__instance.name}' equiped to slot '{__instance.CurrentEquipmentSlot?.SlotType}' for character {_char?.name}. Invoking {nameof(AfterOnEquip)}.");
                AfterOnEquip?.Invoke(_char, __instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(EquipmentPatches)}::{nameof(OnEquipPostfix)}(): Exception disabling quickslots for character {_char?.name}.", ex);
            }
        }
    }
}
