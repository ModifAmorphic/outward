#if DEBUG
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches.Sandbox
{
    [HarmonyPatch(typeof(CharacterEquipment))]
    internal static class CharacterEquipmentPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public delegate void EquipDelegate(Character character, Equipment equipment);
        public static event EquipDelegate AfterEquip;

        [HarmonyPatch(nameof(CharacterEquipment.EquipItem))]
        [HarmonyPatch(new Type[] { typeof(Equipment), typeof(bool) })]
        [HarmonyPostfix]
        private static void EquipPostfix(CharacterEquipment __instance, Character ___m_character, Equipment _itemToEquip, bool _playAnim)
        {
            try
            {
                if (!___m_character.IsLocalPlayer)
                    return;

                Logger.LogTrace($"{nameof(CharacterEquipmentPatches)}::{nameof(EquipPostfix)}(): Invoked. Equipment item '{_itemToEquip.name}' equiped to slot '{_itemToEquip.CurrentEquipmentSlot?.SlotType}' for character {___m_character?.name}. Invoking {nameof(AfterEquip)}.");
                AfterEquip?.Invoke(___m_character, _itemToEquip);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterEquipmentPatches)}::{nameof(EquipPostfix)}(): Exception invoking {nameof(AfterEquip)} for character {___m_character?.name}.", ex);
            }
        }
    }
}
#endif