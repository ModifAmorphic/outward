using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.Network
{
    [RequireComponent(typeof(PhotonView))]
    internal partial class StashPackNet : Photon.PunBehaviour
    {
        private IModifLogger Logger => LoggerFactory?.Invoke();
        public Func<IModifLogger> LoggerFactory { get; set; }

        public static StashPackNet Instance { get; private set; }

        private void OnEnable()
        {
            gameObject.AddComponent<PhotonView>();
            photonView.viewID = 1301000;
        }
        private void Start()
        {
            Instance = this;
        }

        #region overrides

        public override void OnPhotonPlayerConnected(PhotonPlayer _newPlayer) => PlayerConnected?.Invoke(_newPlayer);

        public override void OnLeftRoom() => LeftRoom?.Invoke();


        #endregion

        #region events
        public event Action<StashPackHostSettings> OnReceivedHostSettings;
        public event Action<(string characterUID, string bagUID)> DroppingStashPack;
        public event Action<(string playerUID, string characterUID)> PlayerLeaving;
        public event Action<(string bagUID, string characterUID, bool isLinked)> StashPackLinkChanged;
        public event Action<PhotonPlayer> PlayerConnected;
        public event Action LeftRoom;

        #endregion
        #region PunRPCs
        [PunRPC]
        public void ReceivedStashPackLinkChanged(string bagUID, string characterUID, bool isLinked)
        {
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(ReceivedStashPackLinkChanged)}: Linked status changed to {isLinked} " +
                $"for CharacterUID: {characterUID} StashPack BagUID: {bagUID}.");
            StashPackLinkChanged?.Invoke((bagUID, characterUID, isLinked));
        }
        [PunRPC]
        public void ReceivedDroppingStashPack(string characterUID, string bagUID)
        {
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(ReceivedDroppingStashPack)}:[CharacterUID: {characterUID}]" +
               $" Linked StashPack BagUID: {bagUID}.");
            DroppingStashPack?.Invoke((characterUID, bagUID));
        }
        [PunRPC]
        public void ReceivedPlayerLeaving(string playerUID, string characterUID)
        {
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(ReceivedPlayerLeaving)}:playerUID: {playerUID}," +
               $" characterUID: {characterUID}.");
            PlayerLeaving?.Invoke((playerUID, characterUID));
        }
        [PunRPC]
        public void ReceivedHostSettings(object[] settingsValues)
        {
            var hostSettings = StashPackHostSettings.Deserialize(settingsValues);
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(ReceivedHostSettings)}: " +
                $"{nameof(hostSettings.AllScenesEnabled)}: {hostSettings.AllScenesEnabled}, " +
               $"{nameof(hostSettings.DisableBagScalingRotation)}: {hostSettings.DisableBagScalingRotation}");

            OnReceivedHostSettings?.Invoke(hostSettings);
        }
        #endregion

        public void SendStashPackLinkChanged(string bagUID, string characterUID, bool isLinked)
        {
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(SendStashPackLinkChanged)}:[CharacterUID: {characterUID}]" +
               $" {(isLinked ? string.Empty : "un")}linked StashPack Bag UID: {bagUID}.");
            this.photonView.RPC(nameof(ReceivedStashPackLinkChanged), PhotonTargets.All, bagUID, characterUID, isLinked);
        }
        public void SendLinkedStashPacks(PhotonPlayer target, IEnumerable<(string bagUID, string characterUID)> stashPacks)
        {
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(SendLinkedStashPacks)}: Sending {stashPacks.Count()} linked packs to player {target.NickName}.");
            foreach (var p in stashPacks)
                this.photonView.RPC(nameof(ReceivedStashPackLinkChanged), target, p.bagUID, p.characterUID);
        }
        public void SendDroppingStashPack(string characterUID, string bagUID)
        {
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(SendDroppingStashPack)}:[CharacterUID: {characterUID}]" +
               $" Linked StashPack Bag UID: {bagUID}.");
            this.photonView.RPC(nameof(ReceivedDroppingStashPack), PhotonTargets.Others, characterUID, bagUID);
        }
        public void SendPlayerLeaving(string playerUID, string characterUID)
        {
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(SendPlayerLeaving)}:playerUID: {characterUID}," +
               $" characterUID: {characterUID}.");
            this.photonView.RPC(nameof(ReceivedPlayerLeaving), PhotonTargets.All, playerUID, characterUID);
        }
        public void BufferHostSettings(StashPackHostSettings settings)
        {
            Logger.LogDebug($"{nameof(StashPackNet)}::{nameof(BufferHostSettings)}: " +
                $"{nameof(settings.AllScenesEnabled)}: {settings.AllScenesEnabled}, " +
               $"{nameof(settings.DisableBagScalingRotation)}: {settings.DisableBagScalingRotation}");

            PhotonNetwork.RemoveRPCs(this.photonView);
            this.photonView.RPC(nameof(ReceivedHostSettings), PhotonTargets.AllBuffered, settings.Serialize());
        }
    }
}
