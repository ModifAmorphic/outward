using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings
{
    [HarmonyPatch(typeof(LocalizationManager), "Awake")]
    internal static class QsLocalizationManagerPatches
    {
        static readonly List<ILocalizeListener> _customLocalizationListeners = new List<ILocalizeListener>();

        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        public static void Configure(IEnumerable<ExtendedQuickSlot> extendedQuickSlots)
        {
            var localizations = extendedQuickSlots.ToDictionary(x => x.ActionKey, x => x.ActionDescription);
            var qsLocalizationListener = new MoreQuickslotsLocalizationListener(localizations, Logger);
            _customLocalizationListeners.Add(qsLocalizationListener);
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
