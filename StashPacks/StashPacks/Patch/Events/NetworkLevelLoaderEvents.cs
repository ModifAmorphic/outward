using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class NetworkLevelLoaderEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<NetworkLevelLoader, bool> BaseLoadLevelBefore;

        public static event Action<NetworkLevelLoader> MidLoadLevelBefore;

        public static void RaiseBaseLoadLevelBefore(NetworkLevelLoader networkLevelLoader, bool shouldSave)
        {
            try
            {
                Logger?.LogTrace($"{nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseBaseLoadLevelBefore)} triggered. shouldSave: {shouldSave}");
                BaseLoadLevelBefore?.Invoke(networkLevelLoader, shouldSave);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseBaseLoadLevelBefore)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseBaseLoadLevelBefore)}:\n{ex}");
                }
            }
        }

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
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseMidLoadLevelBefore)}:\n{ex}");
                }
            }
        }
    }
}
