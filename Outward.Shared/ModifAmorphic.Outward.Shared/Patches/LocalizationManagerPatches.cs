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
        private static Func<IModifLogger> _getLogger;
        private static IModifLogger Logger => _getLogger?.Invoke() ?? new NullLogger();
        private static void LoggerEvents_LoggerLoaded(object sender, Func<IModifLogger> getLogger) => _getLogger = getLogger;

        public static event Action<Dictionary<int, ItemLocalization>> LoadItemLocalizationAfter;

        [EventSubscription]
        public static void SubscribeToEvents()
        {
            LoggerEvents.LoggerReady += LoggerEvents_LoggerLoaded;
        }

        [HarmonyPatch("LoadItemLocalization")]
        [HarmonyPostfix]
        private static void LoadItemLocalizationPostfix(ref Dictionary<int, ItemLocalization> ___m_itemLocalization)
        {
            LoadItemLocalizationAfter?.Invoke(___m_itemLocalization);
        }

    }
}
