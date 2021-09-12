using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ModifAmorphic.Outward
{
    public class ServicesProvider
    {
        public BaseUnityPlugin UnityPlugin => GetService<BaseUnityPlugin>();

        private readonly ConcurrentDictionary<Type, Delegate> _serviceFactories = new ConcurrentDictionary<Type, Delegate>();

        private readonly NullLogger nullLogger = new NullLogger();
        private IModifLogger Logger => TryGetService<IModifLogger>(out var logger) ? logger : nullLogger;

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
        public Func<T> GetServiceFactory<T>() => (Func<T>)_serviceFactories[typeof(T)];
        public T GetService<T>() => (T)_serviceFactories[typeof(T)].DynamicInvoke();
        public object GetService(Type type) => _serviceFactories[type].DynamicInvoke();

        public bool TryGetService<T>(out T service)
        {
            service = default(T);
            if (!_serviceFactories.TryGetValue(typeof(T), out var serviceDelegate))
                return false;

            service = (T)serviceDelegate.DynamicInvoke();
            return !EqualityComparer<T>.Default.Equals(service, default(T));
        }
    }
}
