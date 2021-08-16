using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ModifAmorphic.Outward.Extensions;

namespace ModifAmorphic.Outward.StashPacks.Sync
{
    internal class SyncPlanner
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public SyncPlanner(Func<IModifLogger> getLogger) => (_getLogger) = (getLogger);

        /// <summary>
        /// Builds a Plan to sync data from the <paramref name="updatedSilver"/> and <paramref name="saveDataChanges"/> parameters to the <paramref name="targetSaveData"/>'s
        /// <see cref="IContainerSaveData.BasicSaveData"/> and <see cref="IContainerSaveData.ItemsSaveData"/> respectively.
        /// </summary>
        /// <param name="targetSaveData">The target container to build the sync plan against.</param>
        /// <param name="updatedSilver">The new silver amount.</param>
        /// <param name="saveDataChanges">The list of items to sync to the <paramref name="targetSaveData"/>'s <see cref="IContainerSaveData.ItemsSaveData"/>.</param>
        public ContainerSyncPlan PlanSync(IContainerSaveData targetSaveData, int updatedSilver, IEnumerable<BasicSaveData> saveDataChanges)
        {
            Logger.LogInfo($"{nameof(SyncPlanner)}::{nameof(PlanSync)}: Building new plan for character's ({targetSaveData.CharacterUID}) {targetSaveData.Area.GetName()} {targetSaveData.ContainerType.GetName()}. " +
                $"Target container has {targetSaveData.ItemsSaveData.Count()} items and {targetSaveData.BasicSaveData.GetContainerSilver()} silver. " +
                $"Source container has {saveDataChanges?.Count()??0} items and {updatedSilver} silver.");
            var syncPlan = targetSaveData.ToContainerSyncPlan(updatedSilver);

            var saveItems = saveDataChanges?.ToDictionary(s => s.Identifier.ToString(), s => s)?? new Dictionary<string, BasicSaveData>();
            var uidsToRemove = syncPlan.ItemsSaveDataBefore.Keys.Except(saveItems.Keys);
            var uidsToAdd = saveItems.Keys.Except(syncPlan.ItemsSaveDataBefore.Keys);
            var matchedItemUids = saveItems.Keys.Intersect(syncPlan.ItemsSaveDataBefore.Keys);
            var uidsToModify = matchedItemUids.Where(uid => saveItems[uid].SyncData != syncPlan.ItemsSaveDataBefore[uid].SyncData);

            syncPlan.AddedItems = saveItems.Where(kvp => uidsToAdd.Contains(kvp.Key))
                                            .ToDictionary(add => add.Key, add => add.Value);
            syncPlan.RemovedItems = syncPlan.ItemsSaveDataBefore.Where(kvp => uidsToRemove.Contains(kvp.Key))
                                            .ToDictionary(add => add.Key, add => add.Value);
            syncPlan.ModifiedItems = saveItems.Where(kvp => uidsToModify.Contains(kvp.Key))
                                            .ToDictionary(add => add.Key, add => add.Value);


            foreach (var uid in syncPlan.RemovedItems.Keys)
                syncPlan.ItemsSaveDataAfter.Remove(uid);
            foreach (var kvp in syncPlan.AddedItems)
                syncPlan.ItemsSaveDataAfter.Add(kvp.Key, kvp.Value);
            foreach (var kvp in syncPlan.ModifiedItems)
                syncPlan.ItemsSaveDataAfter[kvp.Key] = kvp.Value;

            return syncPlan;
        }
        public void LogSyncPlan(ContainerSyncPlan syncPlan, bool suppressIfEmpty = false)
        {
            var areaName = syncPlan.Area.GetName();
            var sourceContainerType = ContainerTypes.StashPack;
            if (syncPlan.ContainerType == ContainerTypes.StashPack)
                sourceContainerType = ContainerTypes.Stash;

            if (suppressIfEmpty && !syncPlan.HasChanges())
            {
                Logger.LogDebug($"{nameof(SyncPlanner)}::{nameof(PlanSync)}: Plan for '{areaName}' had no changes and {nameof(suppressIfEmpty)} was set to true." +
                    $" Not logging SyncPlan.");
                return;
            }

            var logNoChanges = string.Empty;
            if (!syncPlan.HasChanges())
            {
                logNoChanges = "  ****No Changes were detected for this Sync Plan****\n";
            }
            var logSummary = $"Sync Summary for {areaName} {syncPlan.ContainerType.GetName()}\n" +
                logNoChanges +
                (syncPlan.SaveDataAfter != null ?
                    $"\tChange Silver Amount to: {syncPlan.SaveDataAfter.GetContainerSilver()}\n"
                    : string.Empty) +
                $"\t{syncPlan.ContainerType.GetName()} (Target) Items: {syncPlan.ItemsSaveDataBefore.Count()},  {sourceContainerType.GetName()} (Source) Items: {syncPlan.ItemsSaveDataAfter.Count}\n" +
                $"\tItems to Remove: {syncPlan.RemovedItems.Count()}\n" +
                $"\tItems to Add: {syncPlan.AddedItems.Count()}\n" +
                $"\tItems to Modify: {syncPlan.ModifiedItems.Count()}";
            Logger.LogInfo(logSummary);

            var logSyncPlan = $"Sync Plan Detail for {areaName} {syncPlan.ContainerType.GetName()}\n";
            if (syncPlan.SaveDataAfter != null)
            {
                logSyncPlan += $"  Setting {syncPlan.ContainerType.GetName()}'s Contained Silver:\n";
                logSyncPlan += $"    From: {syncPlan.SaveDataBefore.GetContainerSilver()}\n";
                logSyncPlan += $"      To: {syncPlan.SaveDataAfter.GetContainerSilver()}\n";
            }
            else
            {
                logSyncPlan += $"  No change to {syncPlan.ContainerType.GetName()}'s Contained Silver\n";
            }
            logSyncPlan += $"  Removing {syncPlan.RemovedItems.Count()} Items from {areaName} {syncPlan.ContainerType.GetName()}:\n";
            foreach (var removed in syncPlan.RemovedItems.Values)
                logSyncPlan += $"    Remove: {removed.SyncData}\n";

            logSyncPlan += $"  Adding {syncPlan.AddedItems.Count()} Items to {areaName} {syncPlan.ContainerType.GetName()}\n";
            foreach (var added in syncPlan.AddedItems.Values)
                logSyncPlan += $"    Add: {added.SyncData}\n";

            logSyncPlan += $"  Modifying {syncPlan.ModifiedItems.Count()} Items in {areaName} {syncPlan.ContainerType.GetName()}.\n";
            foreach (var uid in syncPlan.ModifiedItems.Keys)
                logSyncPlan += $"    Before: {syncPlan.ItemsSaveDataBefore[uid].SyncData}\n" +
                               $"     After: {syncPlan.ModifiedItems[uid].SyncData}\n";

            Logger.LogInfo(logSyncPlan);
        }
    }
}
