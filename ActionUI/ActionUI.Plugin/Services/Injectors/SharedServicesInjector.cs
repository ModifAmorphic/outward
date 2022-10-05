using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using System.IO;

namespace ModifAmorphic.Outward.ActionUI.Services.Injectors
{
    public class SharedServicesInjector
    {
        private readonly ServicesProvider _services;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public delegate void SharedServicesInjectedDelegate(int playerID, string characterUID);
        public event SharedServicesInjectedDelegate OnSharedServicesInjected;

        public SharedServicesInjector(ServicesProvider services, Func<IModifLogger> getLogger)
        {
            (_services, _getLogger) = (services, getLogger);

            NetworkInstantiateManagerPatches.BeforeAddLocalPlayer += (manager, playerId, save) => AddSharedServices(playerId, save.CharacterUID);
        }

        public void AddSharedServices(int rewiredID, string characterUID)
        {

            var usp = Psp.Instance.GetServicesProvider(rewiredID);


            usp.AddSingleton<IActionUIProfileService>(new ProfileService(
                                Path.Combine(ActionUISettings.CharactersProfilesPath, characterUID),
                                _services.GetService<GlobalProfileService>(),
                                _getLogger));

            if (!usp.ContainsService<ProfileManager>())
                usp.AddSingleton(new ProfileManager(rewiredID));

            OnSharedServicesInjected?.Invoke(rewiredID, characterUID);

        }

    }
}
