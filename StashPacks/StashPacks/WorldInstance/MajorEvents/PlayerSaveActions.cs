using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.SaveData.Data;
using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents
{
    internal class PlayerSaveActions : MajorBagActions
    {
        public PlayerSaveActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public void SubscribeToEvents()
        {
            //CharacterInventoryEvents.DropBagItemBefore += DropBagItemBefore;
            CharacterSaveInstanceHolderEvents.SaveAfter += SaveAfter;
        }

        private void SaveAfter(CharacterSaveInstanceHolder characterSaveInstanceHolder)
        {
            
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
            foreach(var plan in syncPlans)
            {
                planner.LogSyncPlan(plan);
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
