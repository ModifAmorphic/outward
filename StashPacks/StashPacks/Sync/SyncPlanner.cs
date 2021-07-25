using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var logTargetItems = $"{nameof(SyncPlanner)}::{nameof(PlanSync)}:{targetSaveData.ItemsSaveData.Count()} items in {targetSaveData.Area.GetName()} {targetSaveData.ContainerType.GetName()}.\n";
            ///Don't add if not set to Trace. Adds a decent chunk of processing time building the strings
            ///and writing to disk.
            if (Logger.LogLevel == LogLevel.Trace)
            {
                foreach (var item in targetSaveData.ItemsSaveData)
                    logTargetItems += $"  {item.Identifier}: {item.SyncData}\n";
            }
            Logger.LogDebug(logTargetItems);

#if DEBUG   //Shouldn't ever happen. Used to troubleshoot
            var sourceGroup = from s in saveDataChanges
                              orderby s.Identifier
                              group s by s.Identifier into grp
                              select new { uid = grp.Key, cnt = grp.Count() };
            var dupes = sourceGroup.Where(g => g.cnt > 1);
            var logDupes = $"Found {dupes.Count()} duplicate entries in {nameof(saveDataChanges)}: \n";
            foreach (var d in dupes)
                logDupes += $"UID: {d.uid} - Duplicates: {d.cnt}\n";
            Logger.LogDebug(logDupes);
#endif
            var syncPlan = new ContainerSyncPlan()
            {
                Area = targetSaveData.Area,
                ItemID = targetSaveData.ItemID,
                SaveDataBefore = targetSaveData.BasicSaveData,
                SaveDataAfter = targetSaveData.BasicSaveData.ToUpdatedContainerSilver(updatedSilver)
            };
            syncPlan.ItemsSaveDataBefore = targetSaveData.ItemsSaveData.ToDictionary(s => s.Identifier.ToString(), s => s);
            syncPlan.ItemsSaveDataAfter = targetSaveData.ItemsSaveData.ToDictionary(s => s.Identifier.ToString(), s => s);

            var saveItems = saveDataChanges.ToDictionary(s => s.Identifier.ToString(), s => s);
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
        public void LogSyncPlan(ContainerSyncPlan syncPlan)
        {
            var areaName = syncPlan.Area.GetName();

            Logger.LogInfo($"Sync Summary for {areaName} {syncPlan.ContainerType.GetName()}\n" +
                (syncPlan.SaveDataAfter != null ?
                    $"\tChange Silver Amount to: {syncPlan.SaveDataAfter.GetContainerSilver()}\n"
                    : string.Empty) +
                $"\t{syncPlan.ContainerType.GetName()} Items: {syncPlan.ItemsSaveDataAfter.Count()},  {syncPlan.ContainerType.GetName()} Items: {syncPlan.ItemsSaveDataBefore.Count}\n" +
                $"\tItems to Remove: {syncPlan.RemovedItems.Count()}\n" +
                $"\tItems to Add: {syncPlan.AddedItems.Count()}\n" +
                $"\tItems to Modify: {syncPlan.ModifiedItems.Count()}");

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

            logSyncPlan += $"  Adding {syncPlan.AddedItems.Count()} Items to {areaName} Stash\n";
            foreach (var added in syncPlan.AddedItems.Values)
                logSyncPlan += $"    Add: {added.SyncData}\n";

            logSyncPlan += $"  Modifying {syncPlan.ModifiedItems.Count()} Items in {areaName} {syncPlan.ContainerType.GetName()}.\n";
            foreach (var uid in syncPlan.ModifiedItems.Keys)
                logSyncPlan += $"    Before: {syncPlan.ItemsSaveDataBefore[uid].SyncData}\n" +
                               $"     After: {syncPlan.ModifiedItems[uid].SyncData}\n";

            Logger.LogDebug(logSyncPlan);
        }
    }
}
