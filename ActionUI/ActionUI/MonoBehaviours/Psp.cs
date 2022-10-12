using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class Psp : MonoBehaviour
    {
        private ConcurrentDictionary<int, UnityServicesProvider> services;

        public static Psp Instance { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Awake()
        {
            Instance = this;
            services = new ConcurrentDictionary<int, UnityServicesProvider>();
        }

        public UnityServicesProvider GetServicesProvider(int playerId) => services.GetOrAdd(playerId, new UnityServicesProvider());

        public bool TryGetServicesProvider(int playerId, out UnityServicesProvider usp) => services.TryGetValue(playerId, out usp);

        public bool TryDisposeServicesProvider(int playerId)
        {
            if (services.TryGetValue(playerId, out var disposing))
            {
                try
                {
                    disposing.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"ActionUI: Dispose of {nameof(UnityServicesProvider)} for playerID {playerId} failed.");
                    Debug.LogException(ex);
                }
            }
            return services.TryRemove(playerId, out var removed);
        }
    }
}
