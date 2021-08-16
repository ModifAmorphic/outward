using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.SaveData.Data;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal class PlayerSaveActions : MajorBagActions
    {
        public PlayerSaveActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public override void SubscribeToEvents()
        {
            CharacterSaveInstanceHolderEvents.SaveAfter += SaveAfter;
            SaveInstanceEvents.SaveBefore += SaveBefore;
        }

        private void SaveBefore(SaveInstance saveInstance)
        {
            if (!IsCurrentSceneStashPackEnabled())
            {
                Logger.LogDebug($"{nameof(PlayerSaveActions)}::{nameof(SaveBefore)}: Current Scene is not StashPack Enabled. Not saving for Character '{saveInstance.CharSave.CharacterUID}'.");
                return;
            }
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                Logger.LogDebug($"{nameof(LevelLoadingActions)}::{nameof(SaveBefore)}: Character '{saveInstance.CharSave.CharacterUID}' is not master client. No presave action taken.");
                return;
            }

            if (!_instances.TryGetStashPackWorldData(out var stashPackWorldData)
                || !_instances.TryGetItemManager(out var itemManager))
            {
                Logger.LogError($"{nameof(PlayerSaveActions)}::{nameof(SaveBefore)}: Could not retrieve world data instances.");
                return;
            }
            var stashPacks = stashPackWorldData.GetAllStashPacks();

            //Wipe all stashpack data from the ItemList before they get loaded into the world.

            foreach (var pack in stashPacks)
            {
                if (!BagStateService.IsBagDisabled(pack.StashBag.UID)
                    && pack.StashBag.IsUpdateable())
                {
                    pack.StashBag.EmptyContents();
                    Logger.LogDebug($"{nameof(PlayerSaveActions)}::{nameof(SaveBefore)}: Emptied contents of StashPack Bag '{pack.StashBag.Name}' ({pack.StashBag.UID}).");
                }
                else
                    Logger.LogDebug($"{nameof(PlayerSaveActions)}::{nameof(SaveBefore)}: Bag '{pack.StashBag.Name}' ({pack.StashBag.UID}) is not in an updateable state and will not be emptied.");
            }
        }

        private void SaveAfter(CharacterSaveInstanceHolder characterSaveInstanceHolder)
        {
            if (!IsCurrentSceneStashPackEnabled())
            {
                Logger.LogDebug($"{nameof(PlayerSaveActions)}::{nameof(SaveBefore)}: Current Scene is not StashPack Enabled. Not saving for Character '{characterSaveInstanceHolder.CharacterUID}'.");
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
                Logger.LogError($"{nameof(PlayerSaveActions)}::{nameof(MajorBagActions)}: Could not find stash save data for player '{characterUID}'. " +
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
                    stashExecuter.ExecutePlan(plan, characterSaveInstanceHolder.CurrentSaveInstance);
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
                    Logger.LogError($"{nameof(PlayerSaveActions)}::{nameof(MajorBagActions)}: Could not retrieve {state.Area.GetName()} stash data for " +
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
