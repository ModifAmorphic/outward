using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.UI.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.IO;

namespace ModifAmorphic.Outward.UI.Services.Injectors
{
    internal class InventoryServicesInjector
    {
        private readonly ServicesProvider _provider;
        private bool _isInjected;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public InventoryServicesInjector(ServicesProvider provider, Func<IModifLogger> getLogger)
        {
            (_provider, _getLogger) = (provider, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddSharedServices;
        }

        private void AddSharedServices(SplitPlayer splitPlayer, Character character)
        {
            if (_isInjected)
                return;

            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileManager = usp.GetService<ProfileManager>();

            usp
                .AddSingleton(new InventoryService(
                                                    character,
                                                    profileManager,
                                                    _getLogger));

            _isInjected = true;
        }
    }
}
