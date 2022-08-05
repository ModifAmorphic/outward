using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Services
{
    public interface IServicesProvider
    {
        IServicesProvider AddFactory<T>(Func<T> serviceFactory);
        IServicesProvider AddSingleton<T>(T serviceInstance);
        object GetService(Type type);
        T GetService<T>();
        List<Func<T>> GetServiceFactories<T>();
        Func<T> GetServiceFactory<T>();
        List<T> GetServices<T>();
        bool TryGetService<T>(out T service);
    }
}