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
        private readonly ServicesProvider _services;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly LevelCoroutines _coroutines;

        public InventoryServicesInjector(ServicesProvider services, PlayerMenuService playerMenuService, LevelCoroutines coroutines, Func<IModifLogger> getLogger)
        {
            (_services, _coroutines, _getLogger) = (services, coroutines, getLogger);
            playerMenuService.OnPlayerActionMenusConfigured += AddInventoryServices;
        }

        private void AddInventoryServices(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {

            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileManager = usp.GetService<ProfileManager>();

            usp
                .AddSingleton(new InventoryService(
                                                    splitPlayer.AssignedCharacter,
                                                    profileManager,
                                                    actionMenus.EquipmentSetMenus,
                                                    _coroutines,
                                                    _getLogger))
                .AddSingleton<IEquipmentSetService<WeaponSet>>(new WeaponSetsJsonService(
                    _services.GetService<GlobalProfileService>(),
                    (ProfileService)profileManager.ProfileService,
                    usp.GetService<InventoryService>(),
                    splitPlayer.AssignedCharacter.UID,
                    _getLogger
                    ))
                .AddSingleton<IEquipmentSetService<ArmorSet>>(new ArmorSetsJsonService(
                    _services.GetService<GlobalProfileService>(),
                    (ProfileService)profileManager.ProfileService,
                    usp.GetService<InventoryService>(),
                    splitPlayer.AssignedCharacter.UID,
                    _getLogger
                    ))
                .AddSingleton(new EquipmentMenuStashService(
                    splitPlayer.AssignedCharacter,
                    profileManager,
                    usp.GetService<InventoryService>(),
                    _coroutines,
                    _getLogger
                    ));

            usp.GetService<InventoryService>().Start();
        }
    }
}
