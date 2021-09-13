using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Coroutines
{
    public class PlayerCoroutines : ModifCoroutine
    {
        private readonly BaseUnityPlugin _unityPlugin;
        public PlayerCoroutines(BaseUnityPlugin unityPlugin, Func<IModifLogger> getLogger) : base(getLogger) => _unityPlugin = unityPlugin;

        public void InvokeAfterPlayerLeft(string playerUID, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> playerLeftcondition = () => (!Global.Lobby.PlayersInLobby.Any(p => p.UID == playerUID));
            _unityPlugin.StartCoroutine(base.InvokeAfter(playerLeftcondition, action, timeoutSecs, ticSeconds));
        }
    }
}
