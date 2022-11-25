using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.Patches;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.Services
{
    internal class PlayerServicesDisposer
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly ServicesProvider _services;
        private readonly PlayerServicesProvider _psp;


        public PlayerServicesDisposer(
                                ServicesProvider services,
                                PlayerServicesProvider psp,
                                Func<IModifLogger> getLogger)
        {
            _services = services;
            _psp = psp;
            _getLogger = getLogger;

            CharacterUIPatches.BeforeReleaseUI += DisposePlayerServices; ;
            LobbySystemPatches.BeforeClearPlayerSystems += DisposeAllPlayerServices;
        }

        private void DisposePlayerServices(CharacterUI characterUI, int rewiredId)
        {
            Logger.LogDebug($"Disposing of services for player {rewiredId}");

            try
            {
                _psp.TryDisposeServicesProvider(rewiredId);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Dispose of services failed for player {rewiredId}.", ex);
            }

        }

        private void DisposeAllPlayerServices(LobbySystem lobbySystem)
        {
            var players = lobbySystem.PlayersInLobby.FindAll(p => p.IsLocalPlayer);
            foreach (var p in players)
            {
                DisposePlayerServices(p.ControlledCharacter.CharacterUI, p.PlayerID);
            }
        }
    }
}
