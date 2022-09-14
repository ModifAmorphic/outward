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
    internal class SharedServicesInjector
    {
        private readonly ServicesProvider _provider;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public SharedServicesInjector(ServicesProvider provider, Func<IModifLogger> getLogger)
        {
            (_provider, _getLogger) = (provider, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddSharedServices;
        }

        private void AddSharedServices(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var profileService = new ProfileService(Path.Combine(ActionMenuSettings.ProfilesPath, character.UID), _getLogger);

            psp
                .AddSingleton<IActionMenusProfileService>(profileService);
        }
    }
}
