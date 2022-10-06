using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Rewired;
using System;

namespace ModifAmorphic.Outward.ActionUI.Services.Injectors
{
    internal class HotbarServicesInjector
    {
        private readonly ServicesProvider _provider;
        private PlayerMenuService _playerMenuService;
        private readonly LevelCoroutines _levelCoroutines;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public HotbarServicesInjector(ServicesProvider provider, PlayerMenuService playerMenuService, LevelCoroutines levelCoroutines, Func<IModifLogger> getLogger)
        {
            (_provider, _playerMenuService, _levelCoroutines, _getLogger) = (provider, playerMenuService, levelCoroutines, getLogger);
            _playerMenuService.OnPlayerActionMenusConfigured += TryAddHotbarServices;
        }

        private void TryAddHotbarServices(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {
            try
            {
                AddHotbarServices(actionMenus, splitPlayer);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to enable Action Slots for player {splitPlayer.RewiredID}.", ex);
            }
        }
        private void AddHotbarServices(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {
            Logger.LogDebug($"{nameof(HotbarServicesInjector)}::{nameof(AddHotbarServices)} Beginning Hotbar Services Injection for player {splitPlayer.RewiredID}.");
            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);

            if (usp.ContainsService<HotbarsContainer>())
            {
                Logger.LogDebug($"{nameof(HotbarServicesInjector)}::{nameof(AddHotbarServices)} Hotbars already injected for player {splitPlayer.RewiredID}.");
                return;
            }

            var profileService = usp.GetService<IActionUIProfileService>() as ProfileService;
            var activeProfile = profileService.GetActiveActionUIProfile();

            if (!activeProfile.ActionSlotsEnabled)
            {
                return;
            }

            var hotbars = actionMenus.gameObject.GetComponentInChildren<HotbarsContainer>(true);
            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            usp
                .AddSingleton(hotbars)
                .AddSingleton<IHotbarProfileService>(new HotbarProfileJsonService(profileService
                                              , _getLogger))
                .AddSingleton(new SlotDataService(player
                                        , splitPlayer.AssignedCharacter
                                        , (HotbarProfileJsonService)usp.GetService<IHotbarProfileService>()
                                        , _getLogger))
                .AddSingleton(new HotbarService(hotbars
                                        , player
                                        , splitPlayer.AssignedCharacter
                                        , actionMenus.ProfileManager
                                        , usp.GetService<SlotDataService>()
                                        , _levelCoroutines
                                        , _getLogger))
                .AddSingleton(new ControllerMapService(usp.GetService<HotkeyCaptureMenu>()
                                        , actionMenus.ProfileManager
                                        , usp.GetService<HotbarService>()
                                        , player
                                        , _levelCoroutines
                                        , _getLogger))
                .AddSingleton<IActionViewData>(new SlotActionViewData(player
                                        , splitPlayer.AssignedCharacter
                                        , usp.GetService<SlotDataService>()
                                        , (HotbarProfileJsonService)usp.GetService<IHotbarProfileService>()
                                        , _getLogger))
                .AddSingleton<IHotbarNavActions>(new HotbarKeyListener(player));

            usp.GetService<ControllerMapService>().LoadConfigMaps();

            //_isInjected = true;
            Logger.LogDebug($"{nameof(HotbarServicesInjector)}::{nameof(AddHotbarServices)} Completed Hotbar Services Injection.");
        }

        private void RemoveHotbarServices(UnityServicesProvider usp)
        {
            Logger.LogDebug($"{nameof(HotbarServicesInjector)}::{nameof(RemoveHotbarServices)} Destroying existing Hotbar Services.");
            usp.TryDispose<IHotbarNavActions>();
            usp.TryDispose<IActionViewData>();
            usp.TryDispose<ControllerMapService>();
            usp.TryDispose<HotbarService>();
            usp.TryDispose<SlotDataService>();
            usp.TryDispose<HotbarProfileJsonService>();
            usp.TryDispose<IHotbarProfileService>();
            usp.TryRemove<HotbarsContainer>();
        }
    }
}
