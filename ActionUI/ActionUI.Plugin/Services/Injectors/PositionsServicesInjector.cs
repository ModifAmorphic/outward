using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;

namespace ModifAmorphic.Outward.ActionUI.Services.Injectors
{
    internal class PositionsServicesInjector
    {
        private readonly ServicesProvider _services;
        private readonly ModifGoService _modifGoService;
        private readonly ModifCoroutine _coroutines;
        //private bool _isInjected;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public PositionsServicesInjector(ServicesProvider services, PlayerMenuService playerMenuService, ModifGoService modifGoService, ModifCoroutine coroutines, Func<IModifLogger> getLogger)
        {
            (_services, _modifGoService, _coroutines, _getLogger) = (services, modifGoService, coroutines, getLogger);
            playerMenuService.OnPlayerActionMenusConfigured += TryAddPositionsServices;
        }

        private void TryAddPositionsServices(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {
            try
            {
                AddPositionsServices(actionMenus, splitPlayer);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to enable Positioning UI for player {splitPlayer.RewiredID}.", ex);
            }
        }
        private void AddPositionsServices(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {
            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileService = (ProfileService)usp.GetService<IActionUIProfileService>();

            usp
                .AddSingleton<IPositionsProfileService>(
                    new PositionsProfileJsonService(_services.GetService<GlobalProfileService>(), profileService, splitPlayer.AssignedCharacter.UID, _getLogger));
        }
    }
}
