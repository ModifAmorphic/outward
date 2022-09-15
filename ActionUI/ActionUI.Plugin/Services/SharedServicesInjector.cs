﻿using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Patches;
using ModifAmorphic.Outward.UI.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.IO;

namespace ModifAmorphic.Outward.UI.Services
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
            var profileService = new ProfileService(Path.Combine(ActionUISettings.ProfilesPath, character.UID), _getLogger);

            psp
                .AddSingleton<IActionUIProfileService>(profileService);
        }
    }
}
