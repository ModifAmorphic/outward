using HarmonyLib;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Services;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus
{
    internal class DurabilityDisplayStartup : IStartable
    {
        private readonly Harmony _harmony;
        private readonly ServicesProvider _services;
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public DurabilityDisplayStartup(Harmony harmony, ServicesProvider services, Func<IModifLogger> loggerFactory) =>
            (_harmony, _services, _loggerFactory) = (harmony, services, loggerFactory);
        
        public void Start() 
        {
            Logger.LogInfo("Starting Durability Display...");
            _harmony.PatchAll(typeof(EquipmentPatches));

            _services.AddSingleton(new DurabilityDisplayService(_loggerFactory));

        }
    }
}
