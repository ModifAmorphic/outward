using HarmonyLib;
using Localizer;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Patches
{
    [HarmonyPatch(typeof(LocalizationManager))]
    public static class LocalizationManagerPatches
    {
        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static event Action<LocalizationManager> AwakeAfter;
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void AwakePostfix(LocalizationManager __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(LocalizationManagerPatches)}::{nameof(AwakePostfix)}(): Invoked. Invoking {nameof(AwakePostfix)}({nameof(LocalizationManager)}). AwakeAfter null: {AwakeAfter == null}");
                AwakeAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(LocalizationManagerPatches)}::{nameof(AwakePostfix)}(): Exception Invoking {nameof(AwakePostfix)}({nameof(LocalizationManager)}).", ex);
            }
        }

        public static event Action<LocalizationManager> LoadAfter;
        [HarmonyPatch("Load")]
        [HarmonyPostfix]
        private static void LoadPostfix(LocalizationManager __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(LocalizationManagerPatches)}::{nameof(LoadPostfix)}(): Invoked. Invoking {nameof(LoadAfter)}({nameof(LocalizationManager)}).");
                LoadAfter?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(LocalizationManagerPatches)}::{nameof(LoadPostfix)}(): Exception Invoking {nameof(LoadAfter)}({nameof(LocalizationManager)}).", ex);
            }
        }

        public delegate void RegisterItemLocalizations(ref Dictionary<int, ItemLocalization> itemLocalizations);
        public static event RegisterItemLocalizations LoadItemLocalizationAfter;

        [HarmonyPatch("LoadItemLocalization")]
        [HarmonyPostfix]
        private static void LoadItemLocalizationPostfix(ref Dictionary<int, ItemLocalization> ___m_itemLocalization)
        {
            try
            {
                Logger.LogTrace($"{nameof(LocalizationManagerPatches)}::{nameof(LoadItemLocalizationPostfix)}(): Invoked. Invoking {nameof(LoadItemLocalizationPostfix)}({nameof(Dictionary<int, ItemLocalization>)})");
                LoadItemLocalizationAfter?.Invoke(ref ___m_itemLocalization);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(LocalizationManagerPatches)}::{nameof(LoadItemLocalizationPostfix)}(): Exception Invoking {nameof(LoadItemLocalizationPostfix)}({nameof(Dictionary<int, ItemLocalization>)}).", ex);
            }
        }

    }
}
