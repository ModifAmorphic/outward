using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Localization;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Patches;
using System;
using System.Collections.Concurrent;

namespace ModifAmorphic.Outward.Modules
{
    internal class ModuleService
    {
        Harmony _patcher = null;
        //readonly ConcurrentDictionary<string, Harmony> _modPatchers = new ConcurrentDictionary<string, Harmony>();
        /// <summary>
        /// Collection of classes which have been patched by Harmony
        /// </summary>
        readonly ConcurrentDictionary<Type, byte> _patchedTypes = new ConcurrentDictionary<Type, byte>();
        /// <summary>
        /// Collection of Classes who have had their EventSubscription invoked. Typically Static Patch classes
        /// </summary>
        readonly ConcurrentDictionary<Type, byte> _subscriberTypes = new ConcurrentDictionary<Type, byte>();
        /// <summary>
        /// Collection of modules for each ModId.
        /// </summary>
        readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, IModifModule>> _instances = new ConcurrentDictionary<string, ConcurrentDictionary<Type, IModifModule>>();

        internal T GetModule<T>(string modId, Func<T> factory) where T : class, IModifModule
        {
            var instances = _instances.GetOrAdd(modId, new ConcurrentDictionary<Type, IModifModule>());
            return instances.GetOrAdd(typeof(T), (x) => CreateModule(modId, factory)) as T;
        }

        private IModifModule CreateModule<T>(string modId, Func<T> factory) where T : class, IModifModule
        {
            var module = factory.Invoke();
            LoggerEvents.LoggerCreated += (args) =>
            {
                if (args.ModId == modId)
                    ConfigureMultiLoggersForDependencies<T>(modId, module, () => LoggerFactory.GetLogger(modId));
            };
            ConfigureMultiLoggersForDependencies<T>(modId, module, () => LoggerFactory.GetLogger(modId));
            ApplyPatches(module);
            ConfigureSubscriptions(module, () => LoggerFactory.GetLogger(modId));
            return module;
        }
        internal IModifModule ConfigureSubscriptions(IModifModule module, Func<IModifLogger> getLogger)
        {
            foreach (var t in module.EventSubscriptions)
            {
                if (!_subscriberTypes.ContainsKey(t))
                {
                    _subscriberTypes.TryAdd(t, default);
                    EventSubscriberService.RegisterClassSubscriptions(t, getLogger.Invoke());
                }
            }
            return module;
        }
        private void ConfigureMultiLoggersForDependencies<T>(string modId, IModifModule module, Func<IModifLogger> loggerFactory)
        {
            foreach (var t in module.DepsWithMultiLogger)
                PatchLoggerRegisterService.AddOrUpdatePatchLogger(t, modId, loggerFactory);
        }
        private IModifModule ApplyPatches(IModifModule module)
        {
            foreach (var t in module.PatchDependencies)
            {
                if (!_patchedTypes.ContainsKey(t))
                {
                    _patchedTypes.TryAdd(t, default);
                    GetModPatcher().PatchAll(t);
                    if (t == typeof(LocalizationManagerPatches))
                    {
                        //wake up!
                        LocalizationService.Init();
                    }
                }
            }
            return module;
        }

        private Harmony GetModPatcher()
        {
            return _patcher ?? (_patcher = new Harmony("modifamorphic.outward")); //_modPatchers.GetOrAdd(modId, (x) => new Harmony("modifamorphic.outward"));
        }

    }
}
