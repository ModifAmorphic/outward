using HarmonyLib;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.UI.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Rewired;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.UI.Services
{
    internal class HotbarServicesInjector
    {
        private readonly ServicesProvider _provider;
        //private readonly ModifGoService _modifGoService;
        private readonly LevelCoroutines _levelCoroutines;
        //private readonly HotbarSettings _settings;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public HotbarServicesInjector(ServicesProvider provider, LevelCoroutines levelCoroutines, Func<IModifLogger> getLogger)
        {
            (_provider, _levelCoroutines, _getLogger) = (provider, levelCoroutines, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddHotbarServices;
        }

        private void AddHotbarServices(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileService = psp.GetService<IActionUIProfileService>() as ProfileService;

            if (!profileService.GetActiveActionUIProfile().ActionSlotsEnabled)
            {
                return;
            }

            var playerMenus = psp.GetService<PlayerActionMenus>();
            var hotbars = playerMenus.gameObject.GetComponentInChildren<HotbarsContainer>();
            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            psp
                .AddSingleton(playerMenus.gameObject.GetComponentInChildren<HotbarsContainer>())
                .AddSingleton(hotbars)
                .AddSingleton<IHotbarProfileService>(
                    new HotbarProfileJsonService(profileService
                                              , _getLogger))
                .AddSingleton(new SlotDataService(player
                                        , splitPlayer.AssignedCharacter
                                        , (HotbarProfileJsonService)psp.GetService<IHotbarProfileService>()
                                        , _getLogger))
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
                                        , _getLogger))
                .AddSingleton<IActionViewData>(new SlotActionViewData(player
                                        , splitPlayer.AssignedCharacter
                                        , psp.GetService<SlotDataService>()
                                        , (HotbarProfileJsonService)psp.GetService<IHotbarProfileService>()
                                        , _getLogger))
                .AddSingleton<IHotbarNavActions>(new HotbarKeyListener(player));

            psp.GetService<ControllerMapService>().LoadConfigMaps();

            hotbars.OnAwake += () => _levelCoroutines.DoNextFrame(() => 
                psp.GetService<HotbarService>().Start());
        }
        private void RemoveHotbarServices(int rewiredId)
        {
            var psp = Psp.Instance.GetServicesProvider(rewiredId);

            psp.TryRemove<IHotbarNavActions>();
            psp.TryRemove<IActionViewData>();
            psp.TryRemove<ControllerMapService>();
            psp.TryRemove<HotbarService>();
            psp.TryRemove<SlotDataService>();
            psp.TryRemove<HotbarProfileJsonService>();
            psp.TryRemove<IHotbarProfileService>();
            if (psp.TryGetService<HotbarsContainer>(out var hbc))
            {
                hbc.gameObject.Destroy();
                psp.TryRemove<HotbarsContainer>();
            }
            psp.TryRemove<HotbarsContainer>();
        }
    }
}
