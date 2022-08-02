using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Patches;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ModifAmorphic.Outward.Modules.Items
{
    public class SaveModule : IModifModule
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

        public HashSet<Type> DepsWithMultiLogger => throw new NotImplementedException();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SaveModule(string modId, Func<IModifLogger> loggerFactory)
        {
            this._modId = modId;
            this._loggerFactory = loggerFactory;

        }
    }
}
