using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.UI.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.IO;

namespace ModifAmorphic.Outward.UI.Services.Injectors
{
    internal class SharedServicesInjector
    {
        private readonly ServicesProvider _provider;
        private bool _isInjected;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public SharedServicesInjector(ServicesProvider provider, Func<IModifLogger> getLogger)
        {
            (_provider, _getLogger) = (provider, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddSharedServices;
        }

        private void AddSharedServices(SplitPlayer splitPlayer, Character character)
        {
            if (_isInjected)
                return;

            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileService = new ProfileService(Path.Combine(ActionUISettings.ProfilesPath, character.UID), _getLogger);

            psp
                .AddSingleton(new ProfileManager(splitPlayer.RewiredID))
                .AddSingleton<IActionUIProfileService>(profileService);

            _isInjected = true;
        }
    }
}
