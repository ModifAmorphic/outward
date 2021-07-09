using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.KeyBindings.Patches
{
    [HarmonyPatch(typeof(LocalizationManager), "Awake")]
    static class LocalizationManagerPatches
    {
        static Logger _logger;
        static readonly List<ILocalizeListener> _customLocalizationListeners = new List<ILocalizeListener>();

        private static void LoggerEvents_LoggerLoaded(object sender, Logger logger) => _logger = logger;
        private static void QuickSlotExtenderEvents_SlotsChanged(object sender, QuickSlotExtendedArgs e)
        {
            var localizations = e.ExtendedQuickSlots.ToDictionary(x => x.ActionKey, x => x.ActionDescription);
            var qsLocalizationListener = new MoreQuickslotsLocalizationListener(localizations, _logger);
            _customLocalizationListeners.Add(qsLocalizationListener);
        }

        [EventSubscription]
        public static void SubscribeToEvents()
        {
            LoggerEvents.LoggerLoaded += LoggerEvents_LoggerLoaded;
            QuickSlotExtenderEvents.SlotsChanged += QuickSlotExtenderEvents_SlotsChanged;
        }

        public static void AddLocalizationListener(ILocalizeListener listener)
        {
            _customLocalizationListeners.Add(listener);
        }

        [HarmonyPostfix]
        public static void AddCustomLocalizations(LocalizationManager __instance)
        {
            foreach (var listener in _customLocalizationListeners)
            {
                __instance.RegisterLocalizeElement(listener);
            }
        }
    }
}
