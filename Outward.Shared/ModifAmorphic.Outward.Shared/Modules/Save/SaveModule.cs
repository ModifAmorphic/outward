using Localizer;
using ModifAmorphic.Outward.Patches;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using ModifAmorphic.Outward.Extensions;
using System.Reflection;

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

        private Transform _parentTransform;
        private readonly Dictionary<int, ItemLocalization> _itemLocalizations = new Dictionary<int, ItemLocalization>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SaveModule(string modId, Func<IModifLogger> loggerFactory)
        {
            this._modId = modId;
            this._loggerFactory = loggerFactory;
            
        }
    }
}
