using HarmonyLib;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules
{
    internal class ModuleService
    {
        readonly ConcurrentDictionary<string, Harmony> _modPatchers = new ConcurrentDictionary<string, Harmony>();
        readonly ConcurrentDictionary<Type, byte> _patchedTypes = new ConcurrentDictionary<Type, byte>();
        readonly ConcurrentDictionary<Type, byte> _subscriberTypes = new ConcurrentDictionary<Type, byte>();
        readonly ConcurrentDictionary<Type, IModifModule> _instances = new ConcurrentDictionary<Type, IModifModule>();
        readonly ConcurrentDictionary<string, Func<IModifLogger>> _loggerFactories = new ConcurrentDictionary<string, Func<IModifLogger>>();
        readonly IModifLogger _nullLogger = new NullLogger();

        internal T GetModule<T>(string modId, Func<T> factory) where T : class, IModifModule
        {
            return _instances.GetOrAdd(typeof(T), (x) => {
                var instance = factory.Invoke();
                ConfigureSubscriptions(instance, GetLoggerFactory(modId));
                //Raise the logger again in case the logger was configured before anyone had a chance to react.
                LoggerEvents.RaiseLoggerReady(this, GetLoggerFactory(modId));
                ApplyPatches(instance, modId);
                return instance;
              }
            ) as T;
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
        internal Func<IModifLogger> GetLoggerFactory(string modId)
        {
            _loggerFactories.TryGetValue(modId, out var getLogger);
            return getLogger?? (() => _nullLogger);
        }
        internal void ConfigureLogging(string modId, Func<IModifLogger> loggerFactory)
        {
            _loggerFactories.TryAdd(modId, loggerFactory);
            LoggerEvents.RaiseLoggerReady(this, _loggerFactories[modId]);
        }
        private IModifModule ApplyPatches(IModifModule module, string modId)
        {
            foreach (var t in module.PatchDependencies)
            {
                if (!_patchedTypes.ContainsKey(t))
                {
                    _patchedTypes.TryAdd(t, default);
                    var patcher = GetModPatcher(modId);
                    patcher.PatchAll(t);
                }
            }
            return module;
        }
        private Harmony GetModPatcher(string modId)
        {
            return _modPatchers.GetOrAdd(modId, (x) => new Harmony(modId));
        }
        
    }
}
