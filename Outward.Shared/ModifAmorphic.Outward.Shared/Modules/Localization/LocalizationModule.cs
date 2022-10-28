using ModifAmorphic.Outward.Localization;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Patches;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ModifAmorphic.Outward.Modules.Localization
{
    public class LocalizationModule : IModifModule
    {
        private readonly string _modId;
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(LocalizationManagerPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(LocalizationManagerPatches)
        };

        public HashSet<Type> DepsWithMultiLogger => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(LocalizationService)
        };

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal LocalizationModule(Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
        }

        public void RegisterLocalization(string key, string localization) => LocalizationService.RegisterLocalization(key, localization);
        public static void RegisterLocalizations(IDictionary<string, string> localizations) => LocalizationService.RegisterLocalizations(localizations);
    }
}
