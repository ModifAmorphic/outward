using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Patches;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Localization
{
    public static class LocalizationService
    {
        public static readonly ConcurrentDictionary<string, string> _localizations = new ConcurrentDictionary<string, string>();

        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        private static LocalizationManager _localizationManager;

        internal static void Init()
        {
            LocalizationManagerPatches.AwakeAfter += LocalizationManagerPatches_AwakeAfter;
        }

        private static void LocalizationManagerPatches_AwakeAfter(LocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
            AddLocalizationsToManager();
        }

        public static void RegisterLocalization(string key, string localization)
        {
            _localizations.AddOrUpdate(key, localization, (k, v) => localization);
            
            if (_localizationManager != null)
                AddLocalizationsToManager();
        }
        public static void RegisterLocalizations(IDictionary<string, string> localizations)
        {
            foreach (var kvp in localizations)
            {
                _localizations.AddOrUpdate(kvp.Key, kvp.Value, (k, v) => kvp.Value);
            }
            if (_localizationManager != null)
                AddLocalizationsToManager();
        }
        private static void AddLocalizationsToManager()
        {
            var generalLocs = _localizationManager.GetGeneralLocalizations();
            foreach (var kvp in _localizations)
            {
                (string key, string localization) = (kvp.Key, kvp.Value);
                generalLocs.AddOrUpdate(key, localization);
            }

            //_localizationManager.SetGeneralLocalizations(generalLocs);
            Logger.LogDebug($"{nameof(LocalizationService)}::{nameof(AddLocalizationsToManager)}(): Added or Updated {_localizations.Count} localizations to the {nameof(LocalizationManager)} instance.");
        }
    }
}
