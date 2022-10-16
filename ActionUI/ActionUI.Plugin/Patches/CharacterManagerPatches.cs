#if DEBUG
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches
{
    [HarmonyPatch(typeof(CharacterManager))]
    internal static class CharacterManagerPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<Character> AfterAddCharacter;

        [HarmonyPatch(nameof(CharacterManager.AddCharacter))]
        [HarmonyPatch(new Type[] { typeof(Character) })]
        [HarmonyPostfix]
        private static void AddCharacterPostfix(Character _char)
        {
            try
            {
#if DEBUG
                Logger.LogTrace($"{nameof(CharacterManagerPatches)}::{nameof(AddCharacterPostfix)}(): Invoked. Invoking {nameof(AfterAddCharacter)} for character {_char?.name}.");
#endif
                AfterAddCharacter?.Invoke(_char);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterManagerPatches)}::{nameof(AddCharacterPostfix)}(): Exception Invoking {nameof(AfterAddCharacter)} for character {_char?.name}.", ex);
            }
        }

        //        public static event Action<Character> AfterApplyQuickSlots;

        //        [HarmonyPatch(nameof(CharacterManager.ApplyQuickSlots))]
        //        [HarmonyPatch(new Type[] { typeof(Character) })]
        //        [HarmonyPostfix]
        //        private static void ApplyQuickSlotsPostfix(Character _character)
        //        {
        //            try
        //            {
        //#if DEBUG
        //                Logger.LogTrace($"{nameof(CharacterManagerPatches)}::{nameof(ApplyQuickSlotsPostfix)}(): Invoked. Invoking {nameof(AfterApplyQuickSlots)} for character {_character?.name}.");
        //#endif
        //                AfterApplyQuickSlots?.Invoke(_character);
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.LogException($"{nameof(CharacterManagerPatches)}::{nameof(ApplyQuickSlotsPostfix)}(): Exception Invoking {nameof(AfterApplyQuickSlots)} for character {_character?.name}.", ex);
        //            }
        //        }
    }
}
#endif