using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.UI.Patches
{
    [HarmonyPatch(typeof(CharacterManager))]
    internal static class CharacterManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<Character> AfterApplyQuickSlots;

        [HarmonyPatch(nameof(CharacterManager.ApplyQuickSlots))]
        [HarmonyPatch(new Type[] { typeof(Character) })]
        [HarmonyPostfix]
        private static void ApplyQuickSlotsPostfix(Character _character)
        {
            try
            {
                Logger.LogTrace($"{nameof(CharacterManagerPatches)}::{nameof(ApplyQuickSlotsPostfix)}(): Invoked. Invoking {nameof(AfterApplyQuickSlots)} for character {_character?.name}.");
                AfterApplyQuickSlots?.Invoke(_character);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterManagerPatches)}::{nameof(ApplyQuickSlotsPostfix)}(): Exception Invoking {nameof(AfterApplyQuickSlots)} for character {_character?.name}.", ex);
            }
        }
    }
}
