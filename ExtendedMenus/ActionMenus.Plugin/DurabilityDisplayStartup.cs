using HarmonyLib;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Services;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
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
        private readonly ModifGoService _modifGoService;
        private readonly ModifCoroutine _coroutines;

        private IModifLogger Logger => _loggerFactory.Invoke();

        public DurabilityDisplayStartup(Harmony harmony, ServicesProvider services, ModifGoService modifGoService, ModifCoroutine coroutines, Func<IModifLogger> loggerFactory) =>
            (_harmony, _services, _modifGoService, _coroutines, _loggerFactory) = (harmony, services, modifGoService, coroutines, loggerFactory);
        
        public void Start() 
        {
            Logger.LogInfo("Starting Durability Display...");
            _harmony.PatchAll(typeof(EquipmentPatches));

            _services.AddSingleton(new PositionsServicesInjector(_services, _modifGoService, _coroutines, _loggerFactory))
                     .AddSingleton(new DurabilityDisplayService(_loggerFactory));

        }
    }
}
