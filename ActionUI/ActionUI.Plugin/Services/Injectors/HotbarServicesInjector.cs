using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Rewired;
using System;

namespace ModifAmorphic.Outward.UI.Services.Injectors
{
    internal class HotbarServicesInjector
    {
        private readonly ServicesProvider _provider;
        private readonly LevelCoroutines _levelCoroutines;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public HotbarServicesInjector(ServicesProvider provider, LevelCoroutines levelCoroutines, Func<IModifLogger> getLogger)
        {
            (_provider, _levelCoroutines, _getLogger) = (provider, levelCoroutines, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddHotbarServices;
        }

        private void AddHotbarServices(SplitPlayer splitPlayer, Character character)
        {
            Logger.LogDebug($"{nameof(HotbarServicesInjector)}::{nameof(AddHotbarServices)} Beginning Hotbar Services Injection.");
            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileService = usp.GetService<IActionUIProfileService>() as ProfileService;
            var activeProfile = profileService.GetActiveActionUIProfile();

            if (!activeProfile.ActionSlotsEnabled)
            {
                return;
            }

            var playerMenus = usp.GetService<PlayerActionMenus>();
            var hotbars = playerMenus.gameObject.GetComponentInChildren<HotbarsContainer>();
            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            if (!usp.ContainsService<HotbarsContainer>())
                usp.TryRemove<HotbarsContainer>();
            usp.AddSingleton(hotbars);

            usp.TryDispose<IHotbarProfileService>();
            usp.AddSingleton<IHotbarProfileService>(new HotbarProfileJsonService(profileService
                                              , _getLogger));

            //if (!psp.ContainsService<SlotDataService>())
            usp.TryDispose<SlotDataService>();
            usp.AddSingleton(new SlotDataService(player
                                        , splitPlayer.AssignedCharacter
                                        , (HotbarProfileJsonService)usp.GetService<IHotbarProfileService>()
                                        , _getLogger));

            usp.TryDispose<HotbarService>();
            usp.TryDispose<ControllerMapService>();
            usp
                .AddSingleton(new HotbarService(hotbars
                                        , player
                                        , splitPlayer.AssignedCharacter
                                        , playerMenus.ProfileManager
                                        , usp.GetService<SlotDataService>()
                                        , _levelCoroutines
                                        , _getLogger))
                .AddSingleton(new ControllerMapService(usp.GetService<HotkeyCaptureMenu>()
                                        , playerMenus.ProfileManager
                                        , usp.GetService<HotbarService>()
                                        , player
                                        , _levelCoroutines
                                        , _getLogger));

            usp.TryDispose<IActionViewData>();
            usp.AddSingleton<IActionViewData>(new SlotActionViewData(player
                                        , splitPlayer.AssignedCharacter
                                        , usp.GetService<SlotDataService>()
                                        , (HotbarProfileJsonService)usp.GetService<IHotbarProfileService>()
                                        , _getLogger));

            usp.TryDispose<IHotbarNavActions>();
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
