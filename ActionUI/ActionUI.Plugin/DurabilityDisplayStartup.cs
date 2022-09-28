using HarmonyLib;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Services;
using System;

namespace ModifAmorphic.Outward.ActionUI
{
    internal class DurabilityDisplayStartup : IStartable
    {
        private readonly Harmony _harmony;
        private readonly ServicesProvider _services;
        private readonly Func<IModifLogger> _loggerFactory;
        private readonly ModifGoService _modifGoService;
        private readonly ModifCoroutine _coroutines;
        private readonly string DurabilityModId = ModInfo.ModId + ".DurabilityDisplay";

        private IModifLogger Logger => _loggerFactory.Invoke();

        public DurabilityDisplayStartup(ServicesProvider services, ModifGoService modifGoService, ModifCoroutine coroutines, Func<IModifLogger> loggerFactory)
        {
            (_services, _modifGoService, _coroutines, _loggerFactory) = (services, modifGoService, coroutines, loggerFactory);
            _harmony = new Harmony(DurabilityModId);
        }

        public void Start()
        {
            Logger.LogInfo("Starting Durability Display...");
            _harmony.PatchAll(typeof(EquipmentPatches));

            _services
                     .AddSingleton(new DurabilityDisplayService(_loggerFactory));

        }
    }
}
