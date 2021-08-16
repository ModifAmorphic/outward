using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class NetworkLevelLoaderEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<NetworkLevelLoader> MidLoadLevelBefore;

        public static void RaiseMidLoadLevelBefore(NetworkLevelLoader networkLevelLoader)
        {
            try
            {
                Logger?.LogTrace($"{nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseMidLoadLevelBefore)} triggered.");
                MidLoadLevelBefore?.Invoke(networkLevelLoader);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseMidLoadLevelBefore)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseMidLoadLevelBefore)}:\n{ex}");
            }
        }
    }
}
