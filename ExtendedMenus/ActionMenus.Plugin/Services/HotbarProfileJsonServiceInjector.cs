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
    internal class HotbarProfileJsonServiceInjector
    {
        private readonly HotbarSettings _settings;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public HotbarProfileJsonServiceInjector(HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            (_settings, _getLogger) = (settings, getLogger);
            SplitPlayerPatches.SetCharacterAfter += AddHotbarProfileJsonService;
        }

        private void AddHotbarProfileJsonService(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.GetServicesProvider(splitPlayer.RewiredID);
            psp.AddSingleton<IHotbarProfileDataService>(
                    new HotbarProfileJsonService(Path.Combine(ActionMenuSettings.ProfilesPath, character.UID)
                                              , _settings
                                              , _getLogger)
                );
        }
    }
}
