using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class ConnectPhotonMasterEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<PhotonPlayer> OnPhotonPlayerConnectedAfter;

        public static void RaiseOnPhotonPlayerConnectedAfter(PhotonPlayer photonPlayer)
        {
            try
            {
                Logger.LogDebug($"{nameof(ConnectPhotonMasterEvents)}::{nameof(RaiseOnPhotonPlayerConnectedAfter)}: Player {photonPlayer.NickName} ({photonPlayer.UserId}) connected.");
                OnPhotonPlayerConnectedAfter?.Invoke(photonPlayer);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(ConnectPhotonMasterEvents)}::{nameof(RaiseOnPhotonPlayerConnectedAfter)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(ConnectPhotonMasterEvents)}::{nameof(RaiseOnPhotonPlayerConnectedAfter)}:\n{ex}");
                }
            }
        }
    }
}
