using HarmonyLib;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Services.Injectors;
using System;

namespace ModifAmorphic.Outward.UI
{
    internal class InventoryStartup : IStartable
    {
        private readonly Harmony _harmony;
        private readonly ServicesProvider _services;
        private readonly Func<IModifLogger> _loggerFactory;
        private readonly ModifGoService _modifGoService;
        private readonly ModifCoroutine _coroutines;
        private readonly string ModId = ModInfo.ModId + ".CharacterInventory";

        private IModifLogger Logger => _loggerFactory.Invoke();

        public InventoryStartup(ServicesProvider services, ModifGoService modifGoService, ModifCoroutine coroutines, Func<IModifLogger> loggerFactory)
        {
            (_services, _modifGoService, _coroutines, _loggerFactory) = (services, modifGoService, coroutines, loggerFactory);
            _harmony = new Harmony(ModId);
        }

        public void Start()
        {
            Logger.LogInfo("Starting Inventory Mods...");
            _harmony.PatchAll(typeof(CharacterInventoryPatches));

            _services
                     .AddSingleton(new InventoryServicesInjector(_services, _loggerFactory));

        }
    }
}
