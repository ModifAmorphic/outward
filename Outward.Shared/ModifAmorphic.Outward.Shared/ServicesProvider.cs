using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward
{
    public class ServicesProvider
    {
        public BaseUnityPlugin UnityPlugin => GetService<BaseUnityPlugin>();

        private readonly ConcurrentDictionary<Type, Delegate> _serviceFactories = new ConcurrentDictionary<Type, Delegate>();

        private readonly NullLogger nullLogger = new NullLogger();
        private IModifLogger Logger => TryGetLogger(out var logger) ? logger : nullLogger;

        public ServicesProvider(BaseUnityPlugin unityPlugin)
        {
            AddSingleton(unityPlugin);
        }
        public ServicesProvider AddSingleton<T>(T serviceInstance)
        {
            _serviceFactories.TryAdd(typeof(T), (Func<T>)( () => serviceInstance));
            return this;
        }
        public ServicesProvider AddFactory<T>(Func<T> serviceFactory)
        {
            _serviceFactories.TryAdd(typeof(T), serviceFactory);
            return this;
        }
        public Func<T> GetServiceFactory<T>()
        {
#if DEBUG
            Logger.LogTrace($"{nameof(ServicesProvider)}::{nameof(GetServiceFactory)}<T>: Type: {typeof(T).Name}");
#endif
            return (Func<T>)_serviceFactories[typeof(T)];
        }

        public T GetService<T>()
        {
#if DEBUG
            Logger.LogTrace($"{nameof(ServicesProvider)}::{nameof(GetService)}<T>: Type: {typeof(T).Name}");
#endif
            return (T)_serviceFactories[typeof(T)].DynamicInvoke();
        }

        public object GetService(Type type)
        {
#if DEBUG
            Logger.LogTrace($"{nameof(ServicesProvider)}::{nameof(GetService)}: Type: {type.Name}");
#endif
            return _serviceFactories[type].DynamicInvoke();
        }

        public List<T> GetServices<T>()
        {
#if DEBUG
            Logger.LogTrace($"{nameof(ServicesProvider)}::{nameof(GetServices)}<T>: Type: {typeof(T).Name}");
#endif
            return _serviceFactories
                    .Where(kvp => typeof(T)
                    .IsAssignableFrom(kvp.Key))
                    .Select(kvp => (T)kvp.Value.DynamicInvoke()).ToList();
        }

        public List<Func<T>> GetServiceFactories<T>()
        {
#if DEBUG
            Logger.LogTrace($"{nameof(ServicesProvider)}::{nameof(GetServiceFactories)}<T>: Type: {typeof(T).Name}");
#endif
            return _serviceFactories
                    .Where(kvp => typeof(T)
                    .IsAssignableFrom(kvp.Key))
                    .Select(kvp => (Func<T>)kvp.Value).ToList();
        }

        public bool TryGetService<T>(out T service)
        {
#if DEBUG
            Logger.LogTrace($"{nameof(ServicesProvider)}::{nameof(TryGetService)}<T>: Type: {typeof(T).Name}");
#endif
            service = default(T);
            if (!_serviceFactories.TryGetValue(typeof(T), out var serviceDelegate))
                return false;

            service = (T)serviceDelegate.DynamicInvoke();
            return !EqualityComparer<T>.Default.Equals(service, default(T));
        }
        
        //Exists so TryGetService can log trace events without creating an infinite loop.
        private bool TryGetLogger(out IModifLogger logger)
        {
            logger = null;
            if (!_serviceFactories.TryGetValue(typeof(IModifLogger), out var serviceDelegate))
                return false;

            logger = (IModifLogger)serviceDelegate.DynamicInvoke();
            return logger != null;
        }
    }
}
