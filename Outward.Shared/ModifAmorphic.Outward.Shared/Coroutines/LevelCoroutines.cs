using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.Coroutines
{
    public class LevelCoroutines : ModifCoroutine
    {
        public LevelCoroutines(BaseUnityPlugin unityPlugin, Func<IModifLogger> getLogger) : base(unityPlugin, getLogger) { }

        public void InvokeAfterLevelLoaded(NetworkLevelLoader networkLevelLoader, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> levelLoadedcondition = () => (networkLevelLoader.IsOverallLoadingDone);
            _unityPlugin.StartCoroutine(base.InvokeAfter(levelLoadedcondition, action, timeoutSecs, ticSeconds));
        }
        public void InvokeAfterLevelLoaded(NetworkLevelLoader networkLevelLoader, Func<bool> additonalCondition, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> levelLoadedcondition = () => (networkLevelLoader.IsOverallLoadingDone && additonalCondition());
            _unityPlugin.StartCoroutine(base.InvokeAfter(levelLoadedcondition, action, timeoutSecs, ticSeconds));
        }
        public void InvokeAfterPlayersLoaded(Func<NetworkLevelLoader> getNetworkLevelLoader, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> levelLoadedcondition = () =>
            {
                var levelLoader = getNetworkLevelLoader.Invoke();
                //this.Logger.LogTrace($"{nameof(LevelCoroutines)}.{nameof(InvokeAfterLevelAndPlayersLoaded)}:: AllPlayerDoneLoading=={levelLoader?.AllPlayerDoneLoading}");
                return levelLoader != null && levelLoader.AllPlayerDoneLoading;
            };
            _unityPlugin.StartCoroutine(base.InvokeAfter(levelLoadedcondition, action, timeoutSecs, ticSeconds));
        }
        public void InvokeAfterLevelAndPlayersLoaded(NetworkLevelLoader networkLevelLoader, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds)
        {
            Func<bool> levelLoadedcondition = () =>
            {
                this.Logger.LogTrace($"{nameof(LevelCoroutines)}.{nameof(InvokeAfterLevelAndPlayersLoaded)}:: IsOverallLoadingDone=={networkLevelLoader.IsOverallLoadingDone}\n\t AllPlayerDoneLoading=={networkLevelLoader.AllPlayerDoneLoading}");
                return networkLevelLoader != null && networkLevelLoader.IsOverallLoadingDone && networkLevelLoader.AllPlayerDoneLoading;
            };
            _unityPlugin.StartCoroutine(base.InvokeAfter(levelLoadedcondition, action, timeoutSecs, ticSeconds));
        }
        public void InvokeAfterLevelAndPlayersLoaded(NetworkLevelLoader networkLevelLoader, Func<bool> additonalCondition, Action action, int timeoutSecs, float ticSeconds = DefaultTicSeconds, Func<bool> cancelCondition = null)
        {
            Func<bool> levelLoadedcondition = () => (networkLevelLoader.IsOverallLoadingDone
                                                    && networkLevelLoader.AllPlayerDoneLoading
                                                    && additonalCondition.Invoke());

            _unityPlugin.StartCoroutine(base.InvokeAfter(levelLoadedcondition, action, timeoutSecs, ticSeconds, cancelCondition));
        }
    }
}
