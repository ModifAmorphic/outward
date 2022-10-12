#if DEBUG
using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.ActionUI.Patches.Sandbox
{
    [HarmonyPatch(typeof(CharacterSave))]
    internal static class CharacterSavePatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<Character> BeforeApplyLoadedSaveToCharPrefix;

        [HarmonyPatch(nameof(CharacterSave.ApplyLoadedSaveToChar))]
        [HarmonyPatch(new Type[] { typeof(Character) })]
        [HarmonyPostfix]
        private static void ApplyLoadedSaveToCharPrefix(Character _char)
        {
            try
            {
                Logger.LogTrace($"{nameof(CharacterSavePatches)}::{nameof(ApplyLoadedSaveToCharPrefix)}(): Invoked. Invoking {nameof(BeforeApplyLoadedSaveToCharPrefix)} for character {_char?.name}.");
                BeforeApplyLoadedSaveToCharPrefix?.Invoke(_char);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterSavePatches)}::{nameof(ApplyLoadedSaveToCharPrefix)}(): Exception Invoking {nameof(BeforeApplyLoadedSaveToCharPrefix)} for character {_char?.name}.", ex);
            }
        }
    }
}
#endif