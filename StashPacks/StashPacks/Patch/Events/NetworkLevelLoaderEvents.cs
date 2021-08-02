using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class NetworkLevelLoaderEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<OnJoinedRoomArgs> OnJoinedRoomBefore;
        public static event Action<NetworkLevelLoader> MidLoadLevelBefore;

        public static void RaiseOnJoinedRoomBefore(NetworkLevelLoader networkLevelLoader, bool failedJoin)
        {
            try
            {
                Logger?.LogTrace($"{nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseOnJoinedRoomBefore)} triggered.");
                OnJoinedRoomBefore?.Invoke(new OnJoinedRoomArgs(networkLevelLoader, failedJoin));
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseOnJoinedRoomBefore)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseOnJoinedRoomBefore)}:\n{ex}");
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
                    UnityEngine.Debug.LogError($"Exception in {nameof(NetworkLevelLoaderEvents)}::{nameof(RaiseMidLoadLevelBefore)}:\n{ex}");
            }
        }
    }
}
