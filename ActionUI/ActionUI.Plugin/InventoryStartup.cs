using HarmonyLib;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Services.Injectors;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using System;

namespace ModifAmorphic.Outward.ActionUI
{
    internal class InventoryStartup : IStartable
    {
        private readonly Harmony _harmony;
        private readonly ServicesProvider _services;
        private readonly CraftingMenuEvents _craftingEvents;
        private readonly Func<IModifLogger> _loggerFactory;
        private readonly ModifGoService _modifGoService;
        private readonly ModifCoroutine _coroutines;
        private readonly string ModId = ModInfo.ModId + ".CharacterInventory";

        private IModifLogger Logger => _loggerFactory.Invoke();

        public InventoryStartup(ServicesProvider services, CraftingMenuEvents craftingEvents, ModifGoService modifGoService, ModifCoroutine coroutines, Func<IModifLogger> loggerFactory)
        {
            (_services, _craftingEvents, _modifGoService, _coroutines, _loggerFactory) = (services, craftingEvents, modifGoService, coroutines, loggerFactory);
            _harmony = new Harmony(ModId);
        }

        public void Start()
        {
            Logger.LogInfo("Starting Inventory Mods...");
            _harmony.PatchAll(typeof(CharacterInventoryPatches));
            _harmony.PatchAll(typeof(EquipmentMenuPatches));
            _harmony.PatchAll(typeof(InventoryContentDisplayPatches));
            _harmony.PatchAll(typeof(ItemDisplayPatches));

            _services
                     .AddSingleton(new InventoryServicesInjector(_services, _craftingEvents, _coroutines, _loggerFactory));

        }
    }
}
