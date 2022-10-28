using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Rewired;
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

            NetworkInstantiateManagerPatches.BeforeAddLocalPlayer += (manager, playerId, save) => AddProfileManager(playerId, save.CharacterUID);
            SplitPlayerPatches.SetCharacterAfter += AddSharedServices;
        }

        public void AddProfileManager(int rewiredID, string characterUID)
        {

            var usp = Psp.Instance.GetServicesProvider(rewiredID);

            var profileService = new ProfileService(
                                Path.Combine(ActionUISettings.CharactersProfilesPath, characterUID),
                                _services.GetService<GlobalProfileService>(),
                                _getLogger);
            usp.AddSingleton<IActionUIProfileService>(profileService)
                .AddSingleton<IHotbarProfileService>(new HotbarProfileJsonService(profileService, _getLogger));

            if (!usp.ContainsService<ProfileManager>())
                usp.AddSingleton(new ProfileManager(rewiredID));

            OnSharedServicesInjected?.Invoke(rewiredID, characterUID);

        }

        private void AddSharedServices(SplitPlayer splitPlayer, Character character)
        {
            var usp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);
            var profileService = usp.GetService<IActionUIProfileService>() as ProfileService;

            usp
                .AddSingleton(new SlotDataService(player
                                        , splitPlayer.AssignedCharacter
                                        , (HotbarProfileJsonService)usp.GetService<IHotbarProfileService>()
                                        , _getLogger));
        }

    }
}
