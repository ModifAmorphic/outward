using HarmonyLib;
using Localizer;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Patches
{
    [HarmonyPatch(typeof(LocalizationManager))]
    public static class LocalizationManagerPatches
    {
        [PatchLogger]
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

        public static event Action<Dictionary<int, ItemLocalization>> LoadItemLocalizationAfter;

        [HarmonyPatch("LoadItemLocalization")]
        [HarmonyPostfix]
#pragma warning disable IDE0051 // Remove unused private members
        private static void LoadItemLocalizationPostfix(ref Dictionary<int, ItemLocalization> ___m_itemLocalization)
#pragma warning restore IDE0051 // Remove unused private members
        {
            try
            {
                Logger.LogTrace($"{nameof(LocalizationManagerPatches)}::{nameof(LoadItemLocalizationPostfix)}(): Invoked. Invoking {nameof(LoadItemLocalizationPostfix)}({nameof(Dictionary<int, ItemLocalization>)})");
                LoadItemLocalizationAfter?.Invoke(___m_itemLocalization);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(LocalizationManagerPatches)}::{nameof(LoadItemLocalizationPostfix)}(): Exception Invoking {nameof(LoadItemLocalizationPostfix)}({nameof(Dictionary<int, ItemLocalization>)}).", ex);
            }
        }

    }
}
