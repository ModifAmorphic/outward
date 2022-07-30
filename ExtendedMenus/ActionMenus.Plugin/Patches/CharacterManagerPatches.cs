using HarmonyLib;
using ModifAmorphic.Outward.ActionMenus.Services;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Logging;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Patches
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
                Logger.LogException($"{nameof(CharacterManagerPatches)}::{nameof(ApplyQuickSlotsPostfix)}(): Exception disabling quickslots for character {_character?.name}.", ex);
            }
        }
    }
}
