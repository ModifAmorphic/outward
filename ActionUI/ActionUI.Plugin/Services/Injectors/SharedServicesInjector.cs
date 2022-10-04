using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using System.IO;

namespace ModifAmorphic.Outward.ActionUI.Services.Injectors
{
    internal class SharedServicesInjector
    {
        private readonly ServicesProvider _services;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public delegate void SharedServicesInjectedDelegate(SplitPlayer splitPlayer);
        //public event SharedServicesInjectedDelegate OnSharedServicesInjected;

        public SharedServicesInjector(ServicesProvider services, Func<IModifLogger> getLogger)
        {
            (_services, _getLogger) = (services, getLogger);
            //SplitPlayerPatches.SetCharacterAfter += AddSharedServices;
        }

        public void AddSharedServices(SplitPlayer splitPlayer)
        {

            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);


            usp.AddSingleton<IActionUIProfileService>(new ProfileService(
                                Path.Combine(ActionUISettings.CharactersProfilesPath, splitPlayer.AssignedCharacter.UID),
                                _services.GetService<GlobalProfileService>(),
                                _getLogger));

            if (!usp.ContainsService<ProfileManager>())
                usp.AddSingleton(new ProfileManager(splitPlayer.RewiredID));

            //OnSharedServicesInjected?.Invoke(splitPlayer);

        }

    }
}
