using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings
{
    [HarmonyPatch(typeof(LocalizationManager), "Awake")]
    internal static class LocalizationManagerPatches
    {
        static readonly List<ILocalizeListener> _customLocalizationListeners = new List<ILocalizeListener>();

        [PatchLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        private static void QuickSlotExtenderEvents_SlotsChanged(object sender, QuickSlotExtendedArgs e)
        {
            var localizations = e.ExtendedQuickSlots.ToDictionary(x => x.ActionKey, x => x.ActionDescription);
            var qsLocalizationListener = new MoreQuickslotsLocalizationListener(localizations, Logger);
            _customLocalizationListeners.Add(qsLocalizationListener);
        }

        [EventSubscription]
        public static void SubscribeToEvents()
        {
            //LoggerEvents.LoggerConfigured += LoggerEvents_LoggerLoaded;
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
