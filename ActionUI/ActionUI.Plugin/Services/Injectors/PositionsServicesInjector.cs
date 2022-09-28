using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;

namespace ModifAmorphic.Outward.ActionUI.Services.Injectors
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
            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var actionMenus = usp.GetService<PlayerActionMenus>();
            var profileService = (ProfileService)usp.GetService<IActionUIProfileService>();

            usp.TryDispose<IPositionsProfileService>();
            usp
                .AddSingleton<IPositionsProfileService>(
                    new PositionsProfileJsonService(profileService, _getLogger));

            _isInjected = true;
        }
    }
}
