using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Coroutines
{
    public class LevelCoroutines : ModifCoroutine
    {
        private readonly BaseUnityPlugin _unityPlugin;
        public LevelCoroutines(BaseUnityPlugin unityPlugin, Func<IModifLogger> getLogger) : base(getLogger) => _unityPlugin = unityPlugin;

        public void InvokeAfterLevelLoaded(NetworkLevelLoader networkLevelLoader, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> levelLoadedcondition = () => (networkLevelLoader.IsOverallLoadingDone);
            _unityPlugin.StartCoroutine(base.InvokeAfter(levelLoadedcondition, action, timeoutSecs, ticSeconds));
        }
        public void InvokeAfterLevelAndPlayersLoaded(NetworkLevelLoader networkLevelLoader, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> levelLoadedcondition = () => (networkLevelLoader.IsOverallLoadingDone && networkLevelLoader.AllPlayerDoneLoading);
            _unityPlugin.StartCoroutine(base.InvokeAfter(levelLoadedcondition, action, timeoutSecs, ticSeconds));
        }
    }
}
