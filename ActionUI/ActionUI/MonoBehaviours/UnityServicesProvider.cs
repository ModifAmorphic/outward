﻿using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public class UnityServicesProvider
    {
        private readonly ConcurrentDictionary<Type, Delegate> _serviceFactories = new ConcurrentDictionary<Type, Delegate>();

        public static UnityServicesProvider _instance;
        public static UnityServicesProvider Instance { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake() => _instance = this;

        public UnityServicesProvider AddSingleton<T>(T serviceInstance)
        {
            DebugLogger.Log($"Adding {typeof(T)} singleton with UnityServicesProvider.");
            _serviceFactories.TryAdd(typeof(T), (Func<T>)(() => serviceInstance));
            return this;
        }

        public UnityServicesProvider AddFactory<T>(Func<T> serviceFactory)
        {
            DebugLogger.Log($"Adding {typeof(T)} factory to UnityServicesProvider.");
            _serviceFactories.TryAdd(typeof(T), serviceFactory);
            return this;
        }

        public bool TryRemove<T>() => _serviceFactories.TryRemove(typeof(T), out var _);

        public bool TryDispose<T>()
        {
            bool isDisposed = false;
            if (_serviceFactories.TryGetValue(typeof(T), out var factory))
            {
                var service = factory.DynamicInvoke();

                if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                    isDisposed = true;
                }
            }

            return _serviceFactories.TryRemove(typeof(T), out var _) && isDisposed;
        }

        public Func<T> GetServiceFactory<T>()
        {
            //#if DEBUG
            //            DebugLogger.Log($"{nameof(ServicesProvider)}::{nameof(GetServiceFactory)}<T>: Type: {typeof(T).Name}");
            //#endif
            return (Func<T>)_serviceFactories[typeof(T)];
        }

        public T GetService<T>()
        {
            //#if DEBUG
            //            DebugLogger.Log($"{nameof(ServicesProvider)}::{nameof(GetService)}<T>: Type: {typeof(T).Name}");
            //#endif
            return (T)_serviceFactories[typeof(T)].DynamicInvoke();
        }

        public object GetService(Type type)
        {
            //#if DEBUG
            //            DebugLogger.Log($"{nameof(ServicesProvider)}::{nameof(GetService)}: Type: {type.Name}");
            //#endif
            return _serviceFactories[type].DynamicInvoke();
        }

        public List<T> GetServices<T>()
        {
            //#if DEBUG
            //            DebugLogger.Log($"{nameof(ServicesProvider)}::{nameof(GetServices)}<T>: Type: {typeof(T).Name}");
            //#endif
            return _serviceFactories
                    .Where(kvp => typeof(T)
                    .IsAssignableFrom(kvp.Key))
                    .Select(kvp => (T)kvp.Value.DynamicInvoke()).ToList();
        }

        public List<Func<T>> GetServiceFactories<T>()
        {
            //#if DEBUG
            //            DebugLogger.Log($"{nameof(ServicesProvider)}::{nameof(GetServiceFactories)}<T>: Type: {typeof(T).Name}");
            //#endif
            return _serviceFactories
                    .Where(kvp => typeof(T)
                    .IsAssignableFrom(kvp.Key))
                    .Select(kvp => (Func<T>)kvp.Value).ToList();
        }

        public bool TryGetService<T>(out T service)
        {
            service = default;
            if (!_serviceFactories.TryGetValue(typeof(T), out var serviceDelegate))
                return false;

            service = (T)serviceDelegate.DynamicInvoke();
            return !EqualityComparer<T>.Default.Equals(service, default);
        }

        public bool ContainsService<T>() => _serviceFactories.ContainsKey(typeof(T));
    }
}
