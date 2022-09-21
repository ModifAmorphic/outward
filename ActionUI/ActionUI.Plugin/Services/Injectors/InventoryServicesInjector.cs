using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;

namespace ModifAmorphic.Outward.UI.Services.Injectors
{
    internal class InventoryServicesInjector
    {
        private readonly ServicesProvider _provider;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public InventoryServicesInjector(ServicesProvider provider, Func<IModifLogger> getLogger)
        {
            (_provider, _getLogger) = (provider, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddSharedServices;
        }

        private void AddSharedServices(SplitPlayer splitPlayer, Character character)
        {

            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileManager = usp.GetService<ProfileManager>();

            usp.TryDispose<InventoryService>();
            usp
                .AddSingleton(new InventoryService(
                                                    character,
                                                    profileManager,
                                                    _getLogger));

        }
    }
}
