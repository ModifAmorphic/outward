using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Modules.Crafting.Patches
{
    [HarmonyPatch(typeof(CharacterUI))]
    internal static class CharacterUIPatches
    {
        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<CharacterUI> AwakeBefore;
        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPrefix]
        private static void AwakePrefix(CharacterUI __instance)
        {
            try
            {
                Logger.LogDebug($"{nameof(CharacterUIPatches)}::{nameof(AwakePrefix)}(): Invoked. Invoking {nameof(AwakeBefore)}({nameof(CharacterUI)})");
                AwakeBefore?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterUIPatches)}::{nameof(AwakePrefix)}(): Exception Invoking {nameof(AwakeBefore)}({nameof(CharacterUI)}).", ex);
            }
        }

        public static event Action<CharacterUI, CustomCraftingMenu> RegisterMenuAfter;
        [HarmonyPatch(nameof(CharacterUI.RegisterMenu), MethodType.Normal)]
        [HarmonyPostfix]
        private static void RegisterMenuPostfix(CharacterUI __instance, MenuPanel _menu)
        {
            try
            {
                if (!(_menu is CustomCraftingMenu craftingMenu))
                    return;

                Logger.LogDebug($"{nameof(CharacterUIPatches)}::{nameof(RegisterMenuPostfix)}(): finished. Invoking {nameof(RegisterMenuAfter)}({nameof(CharacterUI)}, {nameof(MenuPanel)})");
                RegisterMenuAfter?.Invoke(__instance, craftingMenu);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CharacterUIPatches)}::{nameof(RegisterMenuPostfix)}(): Exception Invoking {nameof(RegisterMenuAfter)}({nameof(CharacterUI)}, {nameof(MenuPanel)}).", ex);
            }
        }
    }
}
