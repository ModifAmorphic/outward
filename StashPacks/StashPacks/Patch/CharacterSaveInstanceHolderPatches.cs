using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(CharacterSaveInstanceHolder))]
    internal static class CharacterSaveInstanceHolderPatches
    {
        [HarmonyPatch(nameof(CharacterSaveInstanceHolder.ApplyLoadedSaveToChar))]
        [HarmonyPostfix]
        public static void ApplyLoadedSaveToChar(Character _character, CharacterSaveInstanceHolder __instance)
        {
            CharacterSaveInstanceHolderEvents.RaisePlayerSaveLoadedAfter(__instance, _character);
        }
    }
}
