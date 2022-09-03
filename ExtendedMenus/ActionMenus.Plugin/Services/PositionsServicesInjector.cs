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
    internal class PositionsServicesInjector
    {
        private readonly ServicesProvider _provider;
        private readonly ModifGoService _modifGoService;
        private readonly ModifCoroutine _coroutines;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public PositionsServicesInjector(ServicesProvider provider, ModifGoService modifGoService, ModifCoroutine coroutines, Func<IModifLogger> getLogger)
        {
            (_provider, _modifGoService, _coroutines, _getLogger) = (provider, modifGoService, coroutines, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddPositionsServices;
        }

        private void AddPositionsServices(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var playerMenus = psp.GetService<PlayerActionMenus>();
            var hotbars = playerMenus.gameObject.GetComponentInChildren<HotbarsContainer>();
            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);
            var profileService = new ProfileService(Path.Combine(ActionMenuSettings.ProfilesPath, character.UID));

            psp.AddSingleton(hotbars)
                .AddSingleton<IActionMenusProfileService>(profileService)
                .AddSingleton<IPositionsProfileService>(
                    new PositionsProfileJsonService(profileService, _getLogger));
        }
    }
}
