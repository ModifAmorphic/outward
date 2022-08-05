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

        void Awake()
        {
            services = new ConcurrentDictionary<int, UnityServicesProvider>();
            Instance = this;
        }

        public static UnityServicesProvider GetServicesProvider(int playerId) => Instance.services.GetOrAdd(playerId, new UnityServicesProvider());
    }
}
