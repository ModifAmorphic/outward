using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.State;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using System;
using System.Collections.Generic;
using System.Text;
using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
using ModifAmorphic.Outward.StashPacks.Settings;
using System.Linq;
using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using ModifAmorphic.Outward.StashPacks.Sync;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.SaveData.Data;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Models;
using UnityEngine;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions;

namespace ModifAmorphic.Outward.StashPacks
{
    internal class StashPackBehaviors
    {
        private readonly InstanceFactory _instanceFactory;
        private SyncPlanner _syncPlanner => _instanceFactory.GetSyncPlanner();
        private StashPackSaveExecuter _saveSyncExecuter => _instanceFactory.GetStashPackSaveExecuter();


        private readonly Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public StashPackBehaviors(InstanceFactory instanceFactory, Func<IModifLogger> getLogger)
        {
            (_instanceFactory, _getLogger) = (instanceFactory, getLogger);
            RoutePatchEvents();
        }

        private void RoutePatchEvents()
        {
            EnvironmentSaveEvents.ApplyDataBefore += (envSave => SaveDataLoading(ref envSave.ItemList));
            ItemEvents.PerformEquipBefore += BeingEquippedOrTaken;
            CharacterInventoryEvents.DropBagItemAfter += Dropped;
            ItemContainerStaticEvents.BagContentsChangedAfter += ContentsChanged;
        }

        /// <summary>
        /// Used for intercepting and altering a StashPack's data <see cref="List{T}"/> of <see cref="BasicSaveData"/> before it is loaded into the world.
        /// </summary>
        /// <param name="saveDataLoading">The save data to alter.</param>
        private void SaveDataLoading(ref List<BasicSaveData> saveDataLoading)
        {
            Logger.LogTrace($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: {nameof(saveDataLoading)} contains {saveDataLoading?.Count??0} BasicSaveData items.");
            var stashPackSaves = saveDataLoading.Where(s =>
                            s.TryGetPreviousOwnerUID(out _)
                            && s.IsStashPack(StashPacksConstants.StashBackpackItemIds.Keys))
                .ToList();

            Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: There are {stashPackSaves?.Count()??0} StashPacks in {nameof(saveDataLoading)} list. Attemping to reload StashPacks from Home Area Stashes.");

            foreach (var packBasicSave in stashPackSaves)
            {
                Logger.LogTrace($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: {nameof(packBasicSave.SyncData)}: {packBasicSave.SyncData}");
                #region trace logging
                if (Logger.LogLevel == LogLevel.Trace)
                {
                    var ownerResult = packBasicSave.TryGetPreviousOwnerUID(out var logUID);
                    var itemIdResult = packBasicSave.TryGetItemID(out var logItemID);
                    var stashSaveDataResult = _instanceFactory.TryGetStashSaveData(logUID, out var logStashSaveData);
                    Logger.LogTrace($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: Results:\n\t{nameof(BasicSaveDataExtensions.TryGetPreviousOwnerUID)}: [{ownerResult}] '{logUID}'\n" +
                        $"\t{nameof(BasicSaveDataExtensions.TryGetItemID)}: [{itemIdResult}] {logItemID}\n" +
                        $"\t{nameof(InstanceFactory.TryGetStashSaveData)}: [{stashSaveDataResult}] '{logStashSaveData?.SaveFilePath}'"
                        );
                }
                #endregion
                if (!packBasicSave.TryGetPreviousOwnerUID(out var characterUID)
                    || !packBasicSave.TryGetItemID(out var itemID)
                    || StashPacksConstants.StashBackpackItemIds[itemID] == GetCurrentAreaEnum()
                    || !_instanceFactory.TryGetStashSaveData(characterUID, out var stashSaveData))
                {
                    Logger.LogInfo($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: Unable to Sync StashPack. Current Area: '{GetCurrentAreaEnum().GetName()}'. SyncData:\n\t{packBasicSave.SyncData}");
                    return;
                }

                Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: Pack PreviousOwnerUID: {characterUID}, ItemID: {itemID}. Got a valid {nameof(StashSaveData)} instance.");

                var packHomeArea = StashPacksConstants.StashBackpackItemIds[itemID];

                var stashSave = stashSaveData.GetStashSave(packHomeArea);
                var packUID = packBasicSave.Identifier.ToString();
                if (stashSave != null)
                {
                    var stashItems = stashSave.ItemsSaveData.Select(i => i.ToUpdatedHierachy(packUID + StashPacksConstants.BagUidSuffix, itemID));
                    var packItems = saveDataLoading.Where(s => s.GetHierarchyData().ParentUID == packUID + StashPacksConstants.BagUidSuffix);

                    var stashPackSave = new StashPackSave()
                    {
                        Area = StashPacksConstants.StashBackpackItemIds[itemID],
                        BasicSaveData = packBasicSave,
                        ItemID = itemID,
                        UID = packBasicSave.Identifier.ToString(),
                        PreviousOwnerUid = characterUID,
                        ItemsSaveData = packItems
                    };

                    var syncPlan = _syncPlanner.PlanSync(stashPackSave, stashSave.BasicSaveData.GetContainerSilver(), stashItems);

                    _syncPlanner.LogSyncPlan(syncPlan);
                    Logger.LogInfo($"Executing sync plan for {packHomeArea.GetName()} Stash Pack");
                    //Execute the plan
                    _saveSyncExecuter.ExecutePlan(syncPlan, ref saveDataLoading, true);
                }

            }
        }
        private void Dropped(Bag bag)
        {

            if (!StashPacksConstants.StashBackpackItemIds.TryGetValue(bag.ItemID, out var area) || area == GetCurrentAreaEnum())
            {
                Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(Dropped)}: Ignoring bag drop event. Either bag is not a StashPack or StashPack home area is the current area. " +
                    $"{nameof(Bag.ItemID)}={bag.ItemID}, Bag Area='{area.GetName()}', Current Area='{GetCurrentAreaEnum().GetName()}'");
                return;
            }

            if (_instanceFactory.TryGetStashPackWorldData(out var stashPackWorldData))
                _instanceFactory.UnityPlugin.StartCoroutine(stashPackWorldData.GetStashPackWait(bag.UID, SyncStashPack));
        }
        private void SyncStashPack(StashPack stashPack)
        {
            if (!_instanceFactory.TryGetStashPackWorldData(out var stashPackWorldData))
            {
                Logger.LogWarning($"{nameof(StashPackBehaviors)}::{nameof(SyncStashPack)}: Unexpected result.  {nameof(SyncStashPack)} was called, but " +
                    $"no {nameof(stashPackWorldData)} instance could be found. StashPack could not be synced.");
                return;
            }

            if (!stashPack.TryGetStashPackSave(StashPacksConstants.StashBackpackItemIds, out var stashPackSave)
                || !_instanceFactory.TryGetStashSaveData(stashPackSave.PreviousOwnerUid, out var stashSaveData)
                || !_instanceFactory.TryGetStashPackWorldExecuter(out var planExecuter))
            {
                Logger.LogInfo($"{nameof(StashPackBehaviors)}::{nameof(SyncStashPack)}: Could not get {nameof(StashSaveData)} instance for character UID '{stashPackSave.PreviousOwnerUid}'." +
                    $" StashPack could not be synced.");
                return;
            }

            var stashSave = stashSaveData.GetStashSave(stashPack.HomeArea);
            if (stashSave == null)
            {
                Logger.LogError($"{nameof(StashPackBehaviors)}::{nameof(SyncStashPack)}: Could not find a Stash Save for Pack Home Area '{stashPack.HomeArea.GetName()}'. This should never happen. " +
                    $" StashPack will not be synced.");
            }

            var syncPlanner = _instanceFactory.GetSyncPlanner();
            var newParentId = stashPackSave.UID + StashPacksConstants.BagUidSuffix;
            var newStashPackItems = stashSave.ItemsSaveData.Select(s => s.ToUpdatedHierachy(newParentId, stashPack.StashBag.ItemID));
            var syncPlan = syncPlanner.PlanSync(stashPackSave, stashSave.BasicSaveData.GetContainerSilver(), newStashPackItems);
            syncPlanner.LogSyncPlan(syncPlan);
            planExecuter.ExecutePlan(syncPlan);
        }
        private void ContentsChanged(Bag bag, ItemContainerStaticEvents.ContentChanges contentChanges)
        {

        }
        private void BeingEquippedOrTaken(Bag bag)
        {

        }
        //private List<AreaManager.AreaEnum> GetExcludedAreas()
        //{
        //    //return new List<AreaManager.AreaEnum>() { _instanceFactory.AreaManager.CurrentArea. }
        //}
        private AreaManager.AreaEnum GetCurrentAreaEnum()
        {
            var sceneName = _instanceFactory.AreaManager.CurrentArea.SceneName;
            return (AreaManager.AreaEnum)_instanceFactory.AreaManager.GetAreaIndexFromSceneName(sceneName);
        }
    }
}
