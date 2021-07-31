//using ModifAmorphic.Outward.Logging;
//using ModifAmorphic.Outward.StashPacks.Extensions;
//using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
//using ModifAmorphic.Outward.StashPacks.Settings;
//using ModifAmorphic.Outward.StashPacks.Sync.Extensions;
//using ModifAmorphic.Outward.StashPacks.Sync.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ModifAmorphic.Outward.StashPacks.Sync
//{
//    internal class StashPackSaveExecuter
//    {
//        private readonly object _syncLock = new object();

//        private IModifLogger Logger => _getLogger.Invoke();
//        private readonly Func<IModifLogger> _getLogger;

//        public StashPackSaveExecuter(Func<IModifLogger> getLogger) => _getLogger = getLogger;

//        public void ExecutePlan(ContainerSyncPlan syncPlan, ref List<BasicSaveData> listToAlter, bool clearExistingItems = false)
//        {
//            if (!syncPlan.HasChanges())
//            {
//                Logger.LogInfo($"Sync Plan for '{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()} has no changes to execute.");
//                return;
//            }
//            lock (_syncLock)
//            {
//                if (clearExistingItems)
//                {
//                    RemoveAllAndAdd(syncPlan, ref listToAlter);
//                }
//                else
//                {
//                    ModifyExisting(syncPlan, ref listToAlter);
//                }
//            }
//        }
//        private void RemoveAllAndAdd(ContainerSyncPlan syncPlan, ref List<BasicSaveData> listToAlter)
//        {
//            UpdateSilverAmount(syncPlan, ref listToAlter);

//            var removed = listToAlter.RemoveAll(item => {
//                var hierData = item.GetHierarchyData();
//                if (hierData != default)
//                    return hierData.ParentUID.Replace(StashPacksConstants.BagUidSuffix, "") == syncPlan.SaveDataBefore.Identifier.ToString();
//                return false;
//            });

//            Logger.LogDebug($"{nameof(StashPackSaveExecuter)}::{nameof(RemoveAllAndAdd)} Removed all {removed} items in '{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()}.");

//            var upserts = syncPlan.AddedItems.Values.ToList();
//            upserts.AddRange(syncPlan.ModifiedItems.Values);
//            listToAlter.AddRange(upserts);

//            Logger.LogDebug($"{nameof(StashPackSaveExecuter)}::{nameof(RemoveAllAndAdd)} Added {upserts.Count} items to '{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()}.");
//        }
//        private void ModifyExisting(ContainerSyncPlan syncPlan, ref List<BasicSaveData> listToAlter)
//        {
//            UpdateSilverAmount(syncPlan, ref listToAlter);

//            var itemsRemoved = listToAlter.RemoveAll(item => syncPlan.RemovedItems?.ContainsKey(item.Identifier.ToString()) ?? false);
//            Logger.LogDebug($"{nameof(StashPackSaveExecuter)}::{nameof(ModifyExisting)} Removed {itemsRemoved} items from " +
//                $"'{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()}. Tried to remove {syncPlan.RemovedItems.Count} items.");

//            int itemsModified = 0;
//            foreach (var mkvp in syncPlan.ModifiedItems)
//            {
//                var indexOf = listToAlter.FindIndex(i => i.Identifier.ToString() == mkvp.Key);
//                if (indexOf > -1)
//                {
//                    itemsModified++;
//                    listToAlter[indexOf] = mkvp.Value;
//                }
//            }
//            Logger.LogDebug($"{nameof(StashPackSaveExecuter)}::{nameof(ModifyExisting)} Modified {itemsModified} items in " +
//                $"'{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()}. Tried to modify {syncPlan.RemovedItems.Count} items.");

//            listToAlter.AddRange(syncPlan.AddedItems.Values.ToList());

//            Logger.LogDebug($"{nameof(StashPackSaveExecuter)}::{nameof(ModifyExisting)} Added {itemsModified} items to " +
//                $"'{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()}.");
//        }

//        private void UpdateSilverAmount(ContainerSyncPlan syncPlan, ref List<BasicSaveData> listToAlter)
//        {
//            if (syncPlan.HasDifferentSilverAmount())
//            {
//                var indexOfPack = listToAlter.FindIndex(i => i.Identifier.ToString() == syncPlan.SaveDataBefore.Identifier.ToString());
//                listToAlter[indexOfPack] = syncPlan.SaveDataAfter;
//                Logger.LogDebug($"{nameof(StashPackSaveExecuter)}::{nameof(UpdateSilverAmount)} Set '{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()} silver to {syncPlan.SaveDataAfter.GetContainerSilver()}.");
//            }
//        }
//    }
//}
