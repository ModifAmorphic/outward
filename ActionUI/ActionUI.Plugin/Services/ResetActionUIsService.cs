using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class ResetActionUIsService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly LevelCoroutines _coroutine;


        public ResetActionUIsService(
                                LevelCoroutines coroutine,
                                Func<IModifLogger> getLogger)
        {

            _coroutine = coroutine;
            _getLogger = getLogger;

            CharacterUIPatches.BeforeReleaseUI += ResetUIs;
            LobbySystemPatches.BeforeClearPlayerSystems += ResetAllPlayerUIs;
        }

        private void ResetAllPlayerUIs(LobbySystem lobbySystem)
        {
            var players = lobbySystem.PlayersInLobby.FindAll(p => p.IsLocalPlayer);
            foreach (var p in players)
            {
                ResetUIs(p.ControlledCharacter.CharacterUI, p.PlayerID);
            }
        }

        private void ResetUIs(CharacterUI characterUI, int rewiredId)
        {
            Logger.LogDebug($"Destroying Action UIs for player {rewiredId}");
            if (Psp.Instance.TryGetServicesProvider(rewiredId, out var usp) && usp.TryGetService<PositionsService>(out var posService))
            {
                try
                {
                    posService.DestroyPositionableUIs(characterUI);
                }
                catch (Exception ex)
                {
                    Logger.LogException("Dispose of PositionableUIs failed.", ex);
                }
            }

            Psp.Instance.TryDisposeServicesProvider(rewiredId);
            CharacterUIPatches.GetIsMenuFocused.TryRemove(rewiredId, out _);
            
            try
            {
                characterUI.GetComponentInChildren<EquipmentSetMenu>(true).gameObject.Destroy();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Dispose of {nameof(EquipmentSetMenu)} gameobject failed.", ex);
            }
            
            try
            {
                characterUI.GetComponentInChildren<PlayerActionMenus>(true).gameObject.Destroy();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Dispose of {nameof(PlayerActionMenus)} gameobject failed.", ex);
            }
        }
    }
}
