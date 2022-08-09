using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Rewired;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class HotbarServicesInjector
    {
        private readonly ServicesProvider _provider;
        private readonly ModifGoService _modifGoService;
        private readonly LevelCoroutines _levelCoroutines;
        private readonly HotbarSettings _settings;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public HotbarServicesInjector(ServicesProvider provider, ModifGoService modifGoService, LevelCoroutines levelCoroutines, HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            (_provider, _modifGoService, _levelCoroutines, _settings, _getLogger) = (provider, modifGoService, levelCoroutines, settings, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddHotbarServices;
        }

        private void AddHotbarServices(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var playerMenus = psp.GetService<PlayerMenu>().gameObject;
            var hotbars = playerMenus.GetComponentInChildren<HotbarsContainer>();
            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);
            psp.AddSingleton(hotbars)
                .AddSingleton<IHotbarProfileDataService>(new HotbarProfileJsonService(
                                                        Path.Combine(ActionMenuSettings.ProfilesPath, character.UID)
                                                        , _settings
                                                        , _getLogger))
                .AddSingleton(new SlotDataService(player
                                        , splitPlayer.AssignedCharacter
                                        , _getLogger))
                .AddSingleton(new HotbarService(hotbars
                                        , player
                                        , splitPlayer.AssignedCharacter
                                        , psp.GetService<IHotbarProfileDataService>()
                                        , psp.GetService<SlotDataService>()
                                        , _levelCoroutines
                                        , _settings
                                        , _getLogger))
                .AddSingleton(new ControllerMapService(psp.GetService<HotkeyCaptureDialog>()
                                        , (HotbarProfileJsonService)psp.GetService<IHotbarProfileDataService>()
                                        , psp.GetService<HotbarService>()
                                        , player
                                        , _levelCoroutines
                                        , _settings, _getLogger))
                .AddSingleton<IActionViewData>(new SlotActionViewData(player
                                        , splitPlayer.AssignedCharacter
                                        , psp.GetService<SlotDataService>()
                                        , _getLogger))
                .AddSingleton<IHotbarNavActions>(new HotbarKeyListener(player));

            psp.GetService<ControllerMapService>().LoadConfigMaps();
            psp.GetService<HotbarService>().Start();

        }
    }
}
