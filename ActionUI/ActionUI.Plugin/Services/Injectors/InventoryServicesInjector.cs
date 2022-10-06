using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using System;

namespace ModifAmorphic.Outward.ActionUI.Services.Injectors
{
    public class InventoryServicesInjector
    {
        private readonly ServicesProvider _services;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly LevelCoroutines _coroutines;

        public delegate void EquipmentSetProfilesLoadedDelegate(int playerID, string characterUID, ArmorSetsJsonService armorService, WeaponSetsJsonService weaponService);
        public event EquipmentSetProfilesLoadedDelegate EquipmentSetProfilesLoaded;

        public InventoryServicesInjector(ServicesProvider services, PlayerMenuService playerMenuService, LevelCoroutines coroutines, Func<IModifLogger> getLogger)
        {
            (_services, _coroutines, _getLogger) = (services, coroutines, getLogger);
            _services.GetService<SharedServicesInjector>().OnSharedServicesInjected += TryAddServiceProfiles;
            playerMenuService.OnPlayerActionMenusConfigured += TryAddInventoryServices;
        }

        private void TryAddServiceProfiles(int playerID, string characterUID)
        {
            try
            {
                AddServiceProfiles(playerID, characterUID);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to load equipment set profiles for player {playerID}.", ex);
            }
        }

        private void AddServiceProfiles(int playerID, string characterUID)
        {
            var usp = Psp.Instance.GetServicesProvider(playerID);
            var profileManager = usp.GetService<ProfileManager>();

            usp
                .AddSingleton<IEquipmentSetService<WeaponSet>>(new WeaponSetsJsonService(
                    _services.GetService<GlobalProfileService>(),
                    (ProfileService)profileManager.ProfileService,
                    () => usp.GetService<EquipService>(),
                    characterUID,
                    _getLogger
                    ))
                .AddSingleton<IEquipmentSetService<ArmorSet>>(new ArmorSetsJsonService(
                    _services.GetService<GlobalProfileService>(),
                    (ProfileService)profileManager.ProfileService,
                    () => usp.GetService<EquipService>(),
                    characterUID,
                    _getLogger
                    ));

            EquipmentSetProfilesLoaded?.Invoke(
                playerID,
                characterUID,
                (ArmorSetsJsonService)usp.GetService<IEquipmentSetService<ArmorSet>>(),
                (WeaponSetsJsonService)usp.GetService<IEquipmentSetService<WeaponSet>>());
        }

        private void TryAddInventoryServices(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {
            try
            {
                AddInventoryServices(actionMenus, splitPlayer);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to enable Inventory UIs for player {splitPlayer.RewiredID}.", ex);
            }
        }

        private void AddInventoryServices(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {

            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileManager = usp.GetService<ProfileManager>();

            usp
                .AddSingleton(new InventoryService(
                                                    splitPlayer.AssignedCharacter,
                                                    profileManager,
                                                    _coroutines,
                                                    _getLogger))
                .AddSingleton(new EquipService(
                                                    splitPlayer.AssignedCharacter,
                                                    profileManager,
                                                    actionMenus.EquipmentSetMenus,
                                                    usp.GetService<InventoryService>(),
                                                    _services.GetService<EquipSetPrefabService>(),
                                                    _coroutines,
                                                    _getLogger))
                .AddSingleton(new EquipmentMenuStashService(
                    splitPlayer.AssignedCharacter,
                    profileManager,
                    usp.GetService<EquipService>(),
                    _coroutines,
                    _getLogger
                    ));

            usp.GetService<InventoryService>().Start();
            usp.GetService<EquipService>().Start();
        }
    }
}
