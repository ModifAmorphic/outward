using HarmonyLib;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Services;
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
        private readonly PlayerMenuService _playerMenuService;
        private readonly CraftingMenuEvents _craftingEvents;
        private readonly Func<IModifLogger> _loggerFactory;
        private readonly ModifGoService _modifGoService;
        private readonly LevelCoroutines _coroutines;
        private readonly string ModId = ModInfo.ModId + ".CharacterInventory";

        private IModifLogger Logger => _loggerFactory.Invoke();

        public InventoryStartup(ServicesProvider services, PlayerMenuService playerMenuService, CraftingMenuEvents craftingEvents, ModifGoService modifGoService, LevelCoroutines coroutines, Func<IModifLogger> loggerFactory)
        {
            (_services, _playerMenuService, _craftingEvents, _modifGoService, _coroutines, _loggerFactory) = (services, playerMenuService, craftingEvents, modifGoService, coroutines, loggerFactory);
            _harmony = new Harmony(ModId);
        }

        public void Start()
        {
            Logger.LogInfo("Starting Inventory Mods...");
            _harmony.PatchAll(typeof(CharacterInventoryPatches));
            _harmony.PatchAll(typeof(EquipmentMenuPatches));
            _harmony.PatchAll(typeof(InventoryContentDisplayPatches));
            _harmony.PatchAll(typeof(ItemDisplayPatches));
            _harmony.PatchAll(typeof(NetworkLevelLoaderPatches));

            _services
                     .AddSingleton(new InventoryServicesInjector(_services, _playerMenuService, _craftingEvents, _coroutines, _loggerFactory));

        }
    }
}
