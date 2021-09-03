using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.SaveData.Data;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal class PlayerActions : MajorBagActions
    {
        public PlayerActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public override void SubscribeToEvents()
        {
            CharacterSaveInstanceHolderEvents.SaveAfter += SaveAfter;
            SaveInstanceEvents.SaveBefore += SaveBefore;
            //SplitScreenManagerEvents.ReceivedPlayerHasLeftAfter += (photonPlayer, playerUID) => ReceivedPlayerHasLeftAfter(playerUID);
            LobbySystemEvents.PlayerSystemHasBeenDestroyedAfter += ReceivedPlayerHasLeftAfter;
            _instances.StashPackNet.PlayerConnected += PlayerConnected;
            _instances.StashPackNet.LeftRoom += LeftRoom;
        }

        private void LeftRoom()
        {
            _instances.ResetFactory();
            _instances.ResetHostSettings();
        }

        private void PlayerConnected(PhotonPlayer player)
        {
            if (!PhotonNetwork.isNonMasterClientInRoom)
                _instances.StashPackNet.SendLinkedStashPacks(player, BagStateService.GetLinkedBags());
        }

        private void ReceivedPlayerHasLeftAfter(string playerUID)
        {
            var player = GetPlayerSystem(playerUID);
            if (player == null || player.IsHostPlayer())
            {
                return;
            }

            Logger.LogTrace($"{nameof(PlayerActions)}::{nameof(ReceivedPlayerHasLeftAfter)}: playerUID {playerUID} is leaving the game.");
            Logger.LogTrace($"{nameof(PlayerActions)}::{nameof(ReceivedPlayerHasLeftAfter)}: Starting coroutine. Waiting for playerUID '{playerUID}' to leave before cleaning up stash packs.");
            _instances.UnityPlugin.StartCoroutine(
                AfterPlayerLeftCoroutine(playerUID, () => CheckForAbandonedPacks()));

        }
        private void CheckForAbandonedPacks()
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                return;
            }

            var abandonedPacks = GetAbandonedStashPacks();
            foreach (var pack in abandonedPacks)
            {
                UnclaimAndClearBag(pack.StashBag);
            }
        }
        protected IEnumerable<StashPack> GetAbandonedStashPacks()
        {
            if (!_instances.TryGetStashPackWorldData(out var stashPackWorldData))
            {
                Logger.LogError($"{nameof(PlayerActions)}::{nameof(GetAbandonedStashPacks)}: Could not retrieve StashPackWorldData instance.");
                return new List<StashPack>();
            }
            var deployedPacks = stashPackWorldData.GetDeployedStashPacks();
            var abandonedPacks = deployedPacks.Where(p => !IsPlayerCharacterInGame(p.StashBag.PreviousOwnerUID));
            Logger.LogTrace($"{nameof(PlayerActions)}::{nameof(GetAbandonedStashPacks)}: Found {abandonedPacks.Count()} abandoned packs out of {deployedPacks.Count()} deployed.");
            return abandonedPacks;
        }
        private void SaveBefore(SaveInstance saveInstance)
        {
            if (!IsCurrentSceneStashPackEnabled())
            {
                Logger.LogDebug($"{nameof(PlayerActions)}::{nameof(SaveBefore)}: Current Scene is not StashPack Enabled. Not saving for Character '{saveInstance.CharSave.CharacterUID}'.");
                return;
            }
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                Logger.LogDebug($"{nameof(PlayerActions)}::{nameof(SaveBefore)}: Character '{saveInstance.CharSave.CharacterUID}' is not master client. No presave action taken.");
                return;
            }

            if (!_instances.TryGetStashPackWorldData(out var stashPackWorldData)
                || !_instances.TryGetItemManager(out var itemManager))
            {
                Logger.LogError($"{nameof(PlayerActions)}::{nameof(SaveBefore)}: Could not retrieve world data instances.");
                return;
            }
            var stashPacks = stashPackWorldData.GetDeployedStashPacks();

            //Wipe all stashpack data from the ItemList before they get loaded into the world.
            var savePlayer = GetPlayerSystem(saveInstance.CharSave.CharacterUID);

            if (!savePlayer.IsHostPlayer())
            {
                Logger.LogDebug($"{nameof(PlayerActions)}::{nameof(SaveBefore)}: Player of character '{savePlayer.CharUID}' is not the main host. Bags will not be emptied by this player.");
                return;
            }
            var masterCharacterUID = saveInstance.CharSave.CharacterUID;
            foreach (var pack in stashPacks)
            {
                var bag = pack.StashBag;
                // This is a save before host kills the game
                if (!BagStateService.IsBagDisabled(bag.UID)
                    && (Global.IsApplicationClosing || MenuManager.Instance.IsReturningToMainMenu)
                    )
                {
                    //Unclaim all bags that don't belong the the Main player host when exiting
                    if (!bag.IsOwnedBy(masterCharacterUID))
                    {
                        UnclaimAndClearBag(bag);
                    }
                    else
                    {
                        bag.EmptyContents();
                    }

                    Logger.LogDebug($"{nameof(PlayerActions)}::{nameof(SaveBefore)}: Emptied contents of StashPack Bag '{bag.Name}' ({bag.UID}) owned by character {bag.PreviousOwnerUID}." +
                        $"\n\tIsApplicationClosing: {Global.IsApplicationClosing}, IsReturningToMainMenu: {MenuManager.Instance.IsReturningToMainMenu}");
                }
                //Clear local player bags in case they are leaving, which triggers a save of all local characters.
                else if (!BagStateService.IsBagDisabled(bag.UID)
                            && !bag.IsOwnedBy(masterCharacterUID)
                            && IsLocalPlayerCharacter(bag.PreviousOwnerUID))
                {
                    bag.EmptyContents();
                    Logger.LogDebug($"{nameof(PlayerActions)}::{nameof(SaveBefore)}: Emptied contents of StashPack Bag '{bag.Name}' ({bag.UID}) owned by character {bag.PreviousOwnerUID}.");
                }
                else
                {
                    Logger.LogDebug($"{nameof(PlayerActions)}::{nameof(SaveBefore)}: Bag '{bag.Name}' ({bag.UID}) not emptied.");
                }
            }
        }

        private void SaveAfter(CharacterSaveInstanceHolder characterSaveInstanceHolder)
        {
            if (!IsCurrentSceneStashPackEnabled())
            {
                Logger.LogDebug($"{nameof(PlayerActions)}::{nameof(SaveBefore)}: Current Scene is not StashPack Enabled. Not saving for Character '{characterSaveInstanceHolder.CharacterUID}'.");
                return;
            }

            var characterUID = characterSaveInstanceHolder.CharacterUID;
            var bagStateService = _instances.GetBagStateService(characterUID);
            var bagStates = bagStateService.GetAllBagStates();

            if (!bagStates.Any())
            {
                Logger.LogInfo($"No StashPack Bag States found for character '{characterUID}'. Nothing to save.");
                return;
            }

            if (!_instances.TryGetStashSaveData(characterUID, out var stashSaveData))
            {
                Logger.LogError($"{nameof(PlayerActions)}::{nameof(MajorBagActions)}: Could not find stash save data for player '{characterUID}'. " +
                    $"Abandoning StashPack saving. No StashPack changes will be saved to target stashes!");
                return;
            }

            var syncPlans = CreateSyncPlans(characterUID, bagStates, stashSaveData);
            if (syncPlans == null || !syncPlans.Any())
            {
                Logger.LogDebug($"No SyncPlans where created for character '{characterUID}'. Nothing to save.");
                return;
            }

            var stashExecuter = _instances.GetStashSaveExecuter(characterUID, stashSaveData);
            var planner = _instances.GetSyncPlanner();
            foreach (var plan in syncPlans)
            {
                planner.LogSyncPlan(plan);
                if (plan.HasChanges())
                {
                    stashExecuter.ExecutePlan(plan, characterSaveInstanceHolder.CurrentSaveInstance);
                }
            }
        }
        private List<ContainerSyncPlan> CreateSyncPlans(string characterUID, IEnumerable<BagState> bagStates, StashSaveData stashSaveData)
        {

            var syncPlans = new List<ContainerSyncPlan>();
            var syncPlanner = _instances.GetSyncPlanner();

            foreach (var state in bagStates)
            {
                var stashSave = stashSaveData.GetStashSave(state.Area);
                if (stashSave == null)
                {
                    Logger.LogError($"{nameof(PlayerActions)}::{nameof(MajorBagActions)}: Could not retrieve {state.Area.GetName()} stash data for " +
                        $"Character UID '{characterUID}'. Bag State changes will not be saved to this stash!");
                    continue;
                }
                var stashPackItems = state.ItemsSaveData.Select(isd => isd.ToUpdatedHierachy(stashSave.UID, stashSave.ItemID));
                syncPlans.Add(
                    syncPlanner.PlanSync(stashSave, state.BasicSaveData.GetContainerSilver(), stashPackItems)
                    );
            }
            return syncPlans;
        }
    }
}
