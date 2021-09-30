using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    [HarmonyPatch(typeof(CharacterUI))]
    internal static class CharacterUIPatches
    {
        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<CharacterUI> AwakeBefore;
        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPrefix]
        private static void AwakePrefix(CharacterUI __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(CharacterUIPatches)}::{nameof(AwakePrefix)}(): Invoked. Invoking {nameof(AwakeBefore)}({nameof(CharacterUI)})");
                AwakeBefore?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterUIPatches)}::{nameof(AwakePrefix)}(): Exception Invoking {nameof(AwakeBefore)}({nameof(CharacterUI)}).", ex);
            }
        }

        //public static event Action<CharacterUI> StartAfter;
        //[HarmonyPatch("Start", MethodType.Normal)]
        //[HarmonyPostfix]
        //private static void StartPostfix(CharacterUI __instance)
        //{
        //    StartAfter?.Invoke(__instance);
        //}

        //public static event Action<CharacterUI> ShowGameplayPanelBefore;
        //[HarmonyPatch(nameof(CharacterUI.ShowGameplayPanel), MethodType.Normal)]
        //[HarmonyPrefix]
        //private static void ShowGameplayPanelPostfix(CharacterUI __instance)
        //{
        //    ShowGameplayPanelBefore?.Invoke(__instance);
        //}
    }
}
