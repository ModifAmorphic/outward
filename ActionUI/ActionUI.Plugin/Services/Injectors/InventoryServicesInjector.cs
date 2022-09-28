using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using System;

namespace ModifAmorphic.Outward.ActionUI.Services.Injectors
{
    internal class InventoryServicesInjector
    {
        private readonly ServicesProvider _provider;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly CraftingMenuEvents _craftingEvents;
        private readonly ModifCoroutine _coroutines;

        public InventoryServicesInjector(ServicesProvider provider, CraftingMenuEvents craftingEvents, ModifCoroutine coroutines, Func<IModifLogger> getLogger)
        {
            (_provider, _craftingEvents, _coroutines, _getLogger) = (provider, craftingEvents, coroutines, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddInventoryServices;
        }

        private void AddInventoryServices(SplitPlayer splitPlayer, Character character)
        {

            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileManager = usp.GetService<ProfileManager>();
            usp.TryDispose<InventoryService>();
            usp.TryDispose<IEquipmentSetService<ArmorSet>>();
            usp.TryDispose<IEquipmentSetService<WeaponSet>>();
            usp.TryDispose<EquipmentMenuStashService>();

            usp
                .AddSingleton(new InventoryService(
                                                    character,
                                                    profileManager,
                                                    usp.GetService<PlayerActionMenus>().EquipmentSetMenus,
                                                    _coroutines,
                                                    _getLogger))
                .AddSingleton<IEquipmentSetService<WeaponSet>>(new WeaponSetsJsonService(
                    (ProfileService)profileManager.ProfileService,
                    usp.GetService<InventoryService>(),
                    _craftingEvents,
                    _getLogger
                    ))
                .AddSingleton<IEquipmentSetService<ArmorSet>>(new ArmorSetsJsonService(
                    (ProfileService)profileManager.ProfileService,
                    usp.GetService<InventoryService>(),
                    _craftingEvents,
                    _getLogger
                    ))
                .AddSingleton(new EquipmentMenuStashService(
                    character,
                    profileManager,
                    usp.GetService<InventoryService>(),
                    _coroutines,
                    _getLogger
                    ));

            usp.GetService<InventoryService>().Start();
        }
    }
}
