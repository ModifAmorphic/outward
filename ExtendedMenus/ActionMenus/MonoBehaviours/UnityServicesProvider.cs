using ModifAmorphic.Outward.Unity.ActionMenus.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public class UnityServicesProvider
    {
        private readonly ConcurrentDictionary<Type, Delegate> _serviceFactories = new ConcurrentDictionary<Type, Delegate>();

        public static UnityServicesProvider _instance;
        public static UnityServicesProvider Instance { get; private set; }

        private void Awake() => _instance = this;

        public UnityServicesProvider AddSingleton<T>(T serviceInstance)
        {
            _serviceFactories.TryAdd(typeof(T), (Func<T>)(() => serviceInstance));
            return this;
        }

        public UnityServicesProvider AddFactory<T>(Func<T> serviceFactory)
        {
            _serviceFactories.TryAdd(typeof(T), serviceFactory);
            return this;
        }

        public bool TryRemove<T>() => _serviceFactories.TryRemove(typeof(T), out var _);

        public Func<T> GetServiceFactory<T>()
        {
//#if DEBUG
//            Debug.Log($"{nameof(ServicesProvider)}::{nameof(GetServiceFactory)}<T>: Type: {typeof(T).Name}");
//#endif
            return (Func<T>)_serviceFactories[typeof(T)];
        }

        public T GetService<T>()
        {
//#if DEBUG
//            Debug.Log($"{nameof(ServicesProvider)}::{nameof(GetService)}<T>: Type: {typeof(T).Name}");
//#endif
            return (T)_serviceFactories[typeof(T)].DynamicInvoke();
        }

        public object GetService(Type type)
        {
//#if DEBUG
//            Debug.Log($"{nameof(ServicesProvider)}::{nameof(GetService)}: Type: {type.Name}");
//#endif
            return _serviceFactories[type].DynamicInvoke();
        }

        public List<T> GetServices<T>()
        {
//#if DEBUG
//            Debug.Log($"{nameof(ServicesProvider)}::{nameof(GetServices)}<T>: Type: {typeof(T).Name}");
//#endif
            return _serviceFactories
                    .Where(kvp => typeof(T)
                    .IsAssignableFrom(kvp.Key))
                    .Select(kvp => (T)kvp.Value.DynamicInvoke()).ToList();
        }

        public List<Func<T>> GetServiceFactories<T>()
        {
//#if DEBUG
//            Debug.Log($"{nameof(ServicesProvider)}::{nameof(GetServiceFactories)}<T>: Type: {typeof(T).Name}");
//#endif
            return _serviceFactories
                    .Where(kvp => typeof(T)
                    .IsAssignableFrom(kvp.Key))
                    .Select(kvp => (Func<T>)kvp.Value).ToList();
        }

        public bool TryGetService<T>(out T service)
        {
//#if DEBUG
//            Debug.Log($"{nameof(ServicesProvider)}::{nameof(TryGetService)}<T>: Type: {typeof(T).Name}");
//#endif
            service = default;
            if (!_serviceFactories.TryGetValue(typeof(T), out var serviceDelegate))
                return false;

            service = (T)serviceDelegate.DynamicInvoke();
            return !EqualityComparer<T>.Default.Equals(service, default);
        }
    }
    
    //public class PlayerServicesProvider : IServicesProvider
    //{
    //    public int Id { get; }

    //    public PlayerServicesProvider(int playerId) => Id = playerId;

    //    public IServicesProvider AddFactory<T>(Func<T> serviceFactory)
    //    {
    //        if (ServicesProvider.Instance.TryGetService<PlayersService<T>>(out var ps))
    //        {
    //            ps = new PlayersService<T>();
    //            ServicesProvider.Instance.AddFactory<PlayersService<T>>(() => ps);
    //        }

    //        ps.ServiceFactories.AddOrUpdate(Id, serviceFactory, (k, v) => v = serviceFactory);
            
    //        return this;
    //    }

    //    public IServicesProvider AddSingleton<T>(T serviceInstance)
    //    {
    //        if (ServicesProvider.Instance.TryGetService<PlayersService<T>>(out var ps))
    //        {
    //            ps = new PlayersService<T>();
    //            ServicesProvider.Instance.AddFactory<PlayersService<T>>(() => ps);
    //        }

    //        ps.ServiceFactories.AddOrUpdate(Id, () => serviceInstance, (k, v) => v = () => serviceInstance);

    //        return this;
    //    }

    //    public object GetService(Type type)
    //    {
    //        Type serviceClass = typeof(PlayersService<>);
    //        Type serviceType = serviceClass.MakeGenericType(type);

    //        var ps = ServicesProvider.Instance.GetService(serviceType);
    //        var facProperty = serviceType.GetProperty(nameof(PlayersService<string>.ServiceFactories), BindingFlags.Public | BindingFlags.Instance);
            
    //        var factories = (ConcurrentDictionary<int, Func<dynamic>>)facProperty.GetValue(ps);

    //        factories.TryGetValue(Id, out var svcFactory);

    //        return svcFactory.DynamicInvoke();
    //    }

    //    public T GetService<T>()
    //    {
    //        var ps = ServicesProvider.Instance.GetService<PlayersService<T>>();
            
    //        return (T)ps.ServiceFactories[Id].DynamicInvoke();
    //    }

    //    public List<Func<T>> GetServiceFactories<T>()
    //    {
    //        var psf = ServicesProvider.Instance.GetServiceFactories<PlayersService<T>>();

    //        return psf.SelectMany(f => ((PlayersService<T>)f.DynamicInvoke()).ServiceFactories.Values).ToList();
    //    }

    //    public Func<T> GetServiceFactory<T>()
    //    {
    //        var ps = ServicesProvider.Instance.GetService<PlayersService<T>>();

    //        return ps.ServiceFactories[Id];
    //    }

    //    public List<T> GetServices<T>()
    //    {
    //        return GetServiceFactories<T>().Select(f => (T)f.DynamicInvoke()).ToList();
    //    }

    //    public bool TryGetService<T>(out T service)
    //    {
    //        if (ServicesProvider.Instance.TryGetService<PlayersService<T>>(out var ps))
    //        {
    //            if (ps.ServiceFactories.TryGetValue(Id, out var serviceFactory))
    //            {
    //                service = (T)serviceFactory.DynamicInvoke();
    //                return true;
    //            }
    //        }
    //        service = default;
    //        return false;
    //    }
    //}
    //internal class PlayersService<T> 
    //{
    //    public ConcurrentDictionary<int, Func<T>> ServiceFactories { get; } = new ConcurrentDictionary<int, Func<T>>();
    //}

}
