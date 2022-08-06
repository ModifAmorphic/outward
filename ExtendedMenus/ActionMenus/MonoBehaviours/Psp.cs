using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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
            services = new ConcurrentDictionary<int, UnityServicesProvider>();
            Instance = this;
        }

        public static UnityServicesProvider GetServicesProvider(int playerId) => Instance.services.GetOrAdd(playerId, new UnityServicesProvider());
    }
}
