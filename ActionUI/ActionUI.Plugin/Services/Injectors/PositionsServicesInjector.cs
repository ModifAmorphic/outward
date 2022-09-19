using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;

namespace ModifAmorphic.Outward.UI.Services.Injectors
{
    internal class PositionsServicesInjector
    {
        private readonly ServicesProvider _provider;
        private readonly ModifGoService _modifGoService;
        private readonly ModifCoroutine _coroutines;
        private bool _isInjected;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public PositionsServicesInjector(ServicesProvider provider, ModifGoService modifGoService, ModifCoroutine coroutines, Func<IModifLogger> getLogger)
        {
            (_provider, _modifGoService, _coroutines, _getLogger) = (provider, modifGoService, coroutines, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddPositionsServices;
        }

        private void AddPositionsServices(SplitPlayer splitPlayer, Character character)
        {
            if (_isInjected)
                return;
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var actionMenus = psp.GetService<PlayerActionMenus>();
            var profileService = (ProfileService)psp.GetService<IActionUIProfileService>();

            psp
                .AddSingleton<IPositionsProfileService>(
                    new PositionsProfileJsonService(profileService, _getLogger));

            _isInjected = true;
        }
    }
}
