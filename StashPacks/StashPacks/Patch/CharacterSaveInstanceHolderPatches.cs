using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

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
        //[HarmonyPatch(nameof(CharacterSaveInstanceHolder.Save))]
        //[HarmonyPrefix]
        //public static bool SavePrefix(CharacterSaveInstanceHolder __instance)
        //{
        //    CharacterSaveInstanceHolderEvents.RaiseSaveBefore(__instance);
        //    return true;
        //}
        [HarmonyPatch(nameof(CharacterSaveInstanceHolder.Save))]
        [HarmonyPostfix]
        public static void SavePostfix(CharacterSaveInstanceHolder __instance)
        {
            CharacterSaveInstanceHolderEvents.RaiseSaveAfter(__instance);
        }
    }
}
