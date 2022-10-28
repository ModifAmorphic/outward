using Localizer;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Patches;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Localization
{
    public static class LocalizationService
    {
        public static readonly ConcurrentDictionary<string, string> _localizations = new ConcurrentDictionary<string, string>();

        [MultiLogger]
        private static IModifLogger Logger { get; set; } = new NullLogger();

        internal static void Init()
        {
            LocalizationManagerPatches.AwakeAfter += LoadLocalizations;
            LocalizationManagerPatches.LoadAfter += LoadLocalizations;
        }

        private static void LoadLocalizations(LocalizationManager localizationManager)
        {
            Logger.LogDebug("LocalizationService::LoadLocalizations");
            AddLocalizationsToManager();
        }

        public static void RegisterLocalization(string key, string localization)
        {
            Logger.LogDebug($"LocalizationService::RegisterLocalization (\"{key}\", \"{localization}\")");
            _localizations.AddOrUpdate(key, localization, (k, v) => localization);

            AddLocalizationsToManager();
        }
        public static void RegisterLocalizations(IDictionary<string, string> localizations)
        {
            foreach (var kvp in localizations)
            {
                _localizations.AddOrUpdate(kvp.Key, kvp.Value, (k, v) => kvp.Value);
            }
            AddLocalizationsToManager();
        }
        private static void AddLocalizationsToManager()
        {
            if (!TryGetLocalizationManager(out var localizationManager))
                return;

            var generalLocs = localizationManager.GetGeneralLocalizations();
            foreach (var kvp in _localizations)
            {
                (string key, string localization) = (kvp.Key, kvp.Value);
                generalLocs.AddOrUpdate(key, localization);
            }

            //_localizationManager.SetGeneralLocalizations(generalLocs);
            Logger.LogDebug($"{nameof(LocalizationService)}::{nameof(AddLocalizationsToManager)}(): Added or Updated {_localizations.Count} localizations to the {nameof(LocalizationManager)} instance.");
        }

        private static bool TryGetLocalizationManager(out LocalizationManager localizationManager)
        {
            localizationManager = LocalizationManager.Instance;
            
            return localizationManager != null;
        }
    }
}
