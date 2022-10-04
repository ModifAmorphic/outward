﻿using ModifAmorphic.Outward.ActionUI.Patches;
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
            _playerMenuService.OnPlayerActionMenusConfigured += AddHotbarServices;
        }

        private void AddHotbarServices(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {
            Logger.LogDebug($"{nameof(HotbarServicesInjector)}::{nameof(AddHotbarServices)} Beginning Hotbar Services Injection.");
            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileService = usp.GetService<IActionUIProfileService>() as ProfileService;
            var activeProfile = profileService.GetActiveActionUIProfile();

            if (!activeProfile.ActionSlotsEnabled)
            {
                return;
            }

            var hotbars = actionMenus.gameObject.GetComponentInChildren<HotbarsContainer>(true);
            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            if (!usp.ContainsService<HotbarsContainer>())
                usp.AddSingleton(hotbars);

            usp.AddSingleton<IHotbarProfileService>(new HotbarProfileJsonService(profileService
                                              , _getLogger));

            //if (!psp.ContainsService<SlotDataService>())
            usp.AddSingleton(new SlotDataService(player
                                        , splitPlayer.AssignedCharacter
                                        , (HotbarProfileJsonService)usp.GetService<IHotbarProfileService>()
                                        , _getLogger));

            usp
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
                                        , _getLogger));

            usp.AddSingleton<IActionViewData>(new SlotActionViewData(player
                                        , splitPlayer.AssignedCharacter
                                        , usp.GetService<SlotDataService>()
                                        , (HotbarProfileJsonService)usp.GetService<IHotbarProfileService>()
                                        , _getLogger));

            usp.AddSingleton<IHotbarNavActions>(new HotbarKeyListener(player));

            usp.GetService<ControllerMapService>().LoadConfigMaps();

            hotbars.OnAwake += () => _levelCoroutines.DoNextFrame(() =>
                usp.GetService<HotbarService>().Start());

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
