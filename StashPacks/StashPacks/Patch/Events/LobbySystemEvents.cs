using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class LobbySystemEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<string> PlayerSystemHasBeenDestroyedAfter;
        public static event Action OnLeftRoomAfter;
        public static void RaisePlayerSystemHasBeenDestroyedAfter(string playerUID)
        {
            try
            {
                Logger.LogTrace($"{nameof(LobbySystemEvents)}::{nameof(RaisePlayerSystemHasBeenDestroyedAfter)}: PlayerUID: {playerUID}");
                PlayerSystemHasBeenDestroyedAfter?.Invoke(playerUID);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(LobbySystemEvents)}::{nameof(RaisePlayerSystemHasBeenDestroyedAfter)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(LobbySystemEvents)}::{nameof(RaisePlayerSystemHasBeenDestroyedAfter)}:\n{ex}");
                }
            }
        }
        public static void RaiseOnLeftRoomAfter()
        {
            try
            {
                Logger.LogTrace($"{nameof(LobbySystemEvents)}::{nameof(RaiseOnLeftRoomAfter)}: I left.");
                OnLeftRoomAfter?.Invoke();
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(LobbySystemEvents)}::{nameof(RaiseOnLeftRoomAfter)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(LobbySystemEvents)}::{nameof(RaiseOnLeftRoomAfter)}:\n{ex}");
                }
            }
        }
    }
}
