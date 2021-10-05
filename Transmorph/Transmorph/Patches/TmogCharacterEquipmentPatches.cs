using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph
{
    [HarmonyPatch(typeof(CharacterEquipment))]
    internal static class TmogCharacterEquipmentPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<(Character Character, CharacterEquipment CharacterEquipment, Equipment Equipment)> EquipItemBefore;
        [HarmonyPatch(nameof(CharacterEquipment.EquipItem), MethodType.Normal)]
        [HarmonyPatch(new Type[] { typeof(Equipment), typeof(bool) })]
        [HarmonyPrefix]
        private static void EquipItemEquipmentPrefix(CharacterEquipment __instance, Character ___m_character, Equipment _itemToEquip, bool _playAnim)
        {
            try
            {
                Logger.LogTrace($"{nameof(TmogCharacterEquipmentPatches)}::{nameof(EquipItemEquipmentPrefix)}(): Invoked for Equipment Item {_itemToEquip.ItemID} - {_itemToEquip.DisplayName} ({_itemToEquip.UID}). Invoking {nameof(EquipItemBefore)}().");
                EquipItemBefore?.Invoke((___m_character, __instance, _itemToEquip));
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(TmogCharacterEquipmentPatches)}::{nameof(EquipItemEquipmentPrefix)}(): Exception Invoking {nameof(EquipItemBefore)}().", ex);
            }
        }
    }
}
