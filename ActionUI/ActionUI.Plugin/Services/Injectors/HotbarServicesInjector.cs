using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
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
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileService = psp.GetService<IActionUIProfileService>() as ProfileService;
            var activeProfile = profileService.GetActiveActionUIProfile();

            if (!activeProfile.ActionSlotsEnabled)
            {
                return;
            }

            var playerMenus = psp.GetService<PlayerActionMenus>();
            var hotbars = playerMenus.gameObject.GetComponentInChildren<HotbarsContainer>();
            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            if (!psp.ContainsService<HotbarsContainer>())
                psp.AddSingleton(hotbars);

            if (!psp.ContainsService<IHotbarProfileService>())
                psp.AddSingleton<IHotbarProfileService>(new HotbarProfileJsonService(profileService
                                              , _getLogger));

            //if (!psp.ContainsService<SlotDataService>())
            psp.TryDispose<SlotDataService>();
            psp.AddSingleton(new SlotDataService(player
                                        , splitPlayer.AssignedCharacter
                                        , (HotbarProfileJsonService)psp.GetService<IHotbarProfileService>()
                                        , _getLogger));

            psp.TryDispose<HotbarService>();
            psp.TryDispose<ControllerMapService>();

            psp
                .AddSingleton(new HotbarService(hotbars
                                        , player
                                        , splitPlayer.AssignedCharacter
                                        , playerMenus.ProfileManager
                                        , psp.GetService<SlotDataService>()
                                        , _levelCoroutines
                                        , _getLogger))
                .AddSingleton(new ControllerMapService(psp.GetService<HotkeyCaptureMenu>()
                                        , playerMenus.ProfileManager
                                        , psp.GetService<HotbarService>()
                                        , player
                                        , _levelCoroutines
                                        , _getLogger));

            psp.TryDispose<IActionViewData>();
            psp.AddSingleton<IActionViewData>(new SlotActionViewData(player
                                        , splitPlayer.AssignedCharacter
                                        , psp.GetService<SlotDataService>()
                                        , (HotbarProfileJsonService)psp.GetService<IHotbarProfileService>()
                                        , _getLogger));

            psp.TryDispose<IHotbarNavActions>();
            psp.AddSingleton<IHotbarNavActions>(new HotbarKeyListener(player));

            psp.GetService<ControllerMapService>().LoadConfigMaps();

            hotbars.OnAwake += () => _levelCoroutines.DoNextFrame(() =>
                psp.GetService<HotbarService>().Start());

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
