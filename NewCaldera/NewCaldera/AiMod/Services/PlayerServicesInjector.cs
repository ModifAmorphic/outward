using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.Data;
using ModifAmorphic.Outward.NewCaldera.AiMod.Data.Character;
using ModifAmorphic.Outward.NewCaldera.AiMod.Services;
using ModifAmorphic.Outward.NewCaldera.Patches;
using ModifAmorphic.Outward.NewCaldera.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Services
{
    internal class PlayerServicesInjector
    {
        private readonly ServicesProvider _globalServices;
        private readonly PlayerServicesProvider _psp;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public delegate void SharedServicesInjectedDelegate(int playerID, string characterUID);

        public PlayerServicesInjector(ServicesProvider globalServices, PlayerServicesProvider psp, Func<IModifLogger> getLogger)
        {
            (_globalServices, _psp, _getLogger) = (globalServices, psp, getLogger);

            SplitPlayerPatches.SetCharacterAfter += AddPlayerServices;
        }

        private void AddPlayerServices(SplitPlayer splitPlayer, global::Character character)
        {
            if (!character.IsWorldHost)
                return;

            Logger.LogDebug($"Injecting services for player {splitPlayer.RewiredID}, character {character.UID}.");
            var sp = _psp.GetServicesProvider(splitPlayer.RewiredID);
            sp.AddSingleton(new CharacterAiDirectory(Path.Combine(CalderaSettings.ModConfigPath, character.UID),
                                                        _getLogger))
                .AddSingleton(new RegionCyclesData(sp.GetService<CharacterAiDirectory>(), _globalServices.GetService<CyclesHydrator>(), _getLogger))
                .AddSingleton(new AISpawnManager(sp.GetService<RegionCyclesData>(), _getLogger))
                .AddSingleton(new DeathService(sp.GetService<RegionCyclesData>(), _getLogger))
                .AddSingleton(new DropService(sp.GetService<RegionCyclesData>(), _getLogger));
        }
    }
}
