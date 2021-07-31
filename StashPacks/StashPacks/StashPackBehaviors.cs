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
using ModifAmorphic.Outward.StashPacks.WorldInstance.Data;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using ModifAmorphic.Outward.StashPacks.Sync.Extensions;

namespace ModifAmorphic.Outward.StashPacks
{
    internal class StashPackBehaviors
    {
        private readonly InstanceFactory _instanceFactory;
        private readonly StashBagState _stashBagState;
        private SyncPlanner _syncPlanner => _instanceFactory.GetSyncPlanner();
        private StashPackSaveExecuter _saveSyncExecuter => _instanceFactory.GetStashPackSaveExecuter();


        private readonly Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public StashPackBehaviors(InstanceFactory instanceFactory, StashBagState stashBagState, Func<IModifLogger> getLogger)
        {
            (_instanceFactory, _stashBagState, _getLogger) = (instanceFactory, stashBagState, getLogger);
            RoutePatchEvents();
        }

        private void RoutePatchEvents()
        {
            EnvironmentSaveEvents.ApplyDataBefore += (envSave => SaveDataLoading(ref envSave.ItemList));
            ItemEvents.PerformEquipBefore += BeingEquippedOrTaken;
            CharacterInventoryEvents.DropBagItemAfter += Dropped;
            //ItemContainerStaticEvents.BagContentsChangedAfter += ContentsChanged;
            ItemContainerEvents.RefreshWeightAfter += ContentsChanged;
        }
        #region StashPack Updating
        /// <summary>
        /// Used for intercepting and altering a StashPack's data <see cref="List{T}"/> of <see cref="BasicSaveData"/> before it is loaded into the world.
        /// </summary>
        /// <param name="saveDataLoading">The save data to alter.</param>
        private void SaveDataLoading(ref List<BasicSaveData> saveDataLoading)
        {
            Logger.LogTrace($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: {nameof(saveDataLoading)} contains {saveDataLoading?.Count ?? 0} BasicSaveData items.");
            var stashPackSaves = saveDataLoading.Where(s =>
                            s.TryGetPreviousOwnerUID(out _)
                            && s.IsStashPack(StashPacksConstants.StashBackpackItemIds.Keys))
                .ToList();

            Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: There are {stashPackSaves?.Count() ?? 0} StashPacks in {nameof(saveDataLoading)} list. Attemping to reload StashPacks from Home Area Stashes.");

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
                    || !_instanceFactory.TryGetStashSaveData(characterUID, out var stashSaveData))
                {
                    Logger.LogWarning($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: Unable to Sync StashPack. Current Area: '{GetCurrentAreaEnum().GetName()}'. SyncData:\n\t{packBasicSave.SyncData}");
                    return;
                }
                if (StashPacksConstants.StashBackpackItemIds[itemID] == GetCurrentAreaEnum())
                {
                    Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: Skipping Sync of StashPack '{packBasicSave.Identifier}'. StashPack's HomeArea is that of the current area '{GetCurrentAreaEnum().GetName()}'. SyncData:\n\t{packBasicSave.SyncData}");
                }

                Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(SaveDataLoading)}: Pack PreviousOwnerUID: {characterUID}, ItemID: {itemID}. Got a valid {nameof(StashSaveData)} instance.");

                var packHomeArea = StashPacksConstants.StashBackpackItemIds[itemID];

                var stashSave = stashSaveData.GetStashSave(packHomeArea);
                var packUID = packBasicSave.Identifier.ToString();
                if (stashSave != null)
                {
                    var packItems = saveDataLoading.Where(s => s.GetHierarchyData().ParentUID == packUID + StashPacksConstants.BagUidSuffix).ToList();

                    //Bag items parent is actually the itemid of the container of the bag, not the bag's itemID itself. Although, using the bag ID works
                    // it will create false positives when checking for modifications between current bag items and stash items. So, default to the
                    // backpack ItemID just in case the pack doesn't have any items to extract the parent container id from.
                    var packContainerItemId = itemID;
                    var firstItem = packItems.FirstOrDefault();
                    if (firstItem != default)
                        packContainerItemId = firstItem.GetHierarchyData().ParentItemID;

                    var stashItems = stashSave.ItemsSaveData.Select(i => i.ToUpdatedHierachy(packUID + StashPacksConstants.BagUidSuffix, packContainerItemId));
                    var stashPackSave = new StashPackSave()
                    {
                        Area = StashPacksConstants.StashBackpackItemIds[itemID],
                        BasicSaveData = packBasicSave.ToClone(),
                        ItemID = itemID,
                        UID = packBasicSave.Identifier.ToString(),
                        CharacterUID = characterUID,
                        ItemsSaveData = packItems
                    };

                    var syncPlan = _syncPlanner.PlanSync(stashPackSave, stashSave.BasicSaveData.GetContainerSilver(), stashItems);

                    _syncPlanner.LogSyncPlan(syncPlan);
                    if (syncPlan.HasChanges())
                    {
                        Logger.LogInfo($"Executing sync plan for {packHomeArea.GetName()} Stash Pack");
                        //Execute the plan
                        _stashBagState.DisableSyncing(syncPlan.CharacterUID, stashPackSave.UID);
                        _saveSyncExecuter.ExecutePlan(syncPlan, ref saveDataLoading, true);
                    }
                }
            }
        }
        private void Dropped(Character character, Bag bag)
        {
            var characterUID = character.UID.ToString();
            Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(Dropped)}: Handling Drop bag event for {character.UID}, bag '{bag.Name}' ItemID: ({bag.ItemID}).");
            if (!_stashBagState.IsAvailableForSyncing(characterUID, bag.UID))
            {
                Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(Dropped)}: Ignoring bag drop event. Bag '{bag.Name}' ({bag.ItemID}) is currently not available for syncing.");
                return;
            }
            if (!StashPacksConstants.StashBackpackItemIds.TryGetValue(bag.ItemID, out var area) || area == GetCurrentAreaEnum())
            {
                Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(Dropped)}: Ignoring bag drop event. Either bag is not a StashPack or StashPack home area is the current area. " +
                    $"{nameof(Bag.ItemID)}={bag.ItemID}, Bag Area='{area.GetName()}', Current Area='{GetCurrentAreaEnum().GetName()}'");
                return;
            }

            if (_instanceFactory.TryGetStashPackWorldData(out var stashPackWorldData))
            {
                _stashBagState.DisableSyncing(characterUID, bag.UID);
                _instanceFactory.UnityPlugin.StartCoroutine(stashPackWorldData.InvokeAfterStashPackLoaded(bag.UID, SyncStashPack));
            }
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
                || !_instanceFactory.TryGetStashSaveData(stashPackSave.CharacterUID, out var stashSaveData)
                || !_instanceFactory.TryGetStashPackWorldExecuter(out var planExecuter))
            {
                Logger.LogInfo($"{nameof(StashPackBehaviors)}::{nameof(SyncStashPack)}: Could not get {nameof(StashSaveData)} instance for character UID '{stashPackSave.CharacterUID}'." +
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
        #endregion
        
        #region Stash Updating
        private void ContentsChanged(Bag bag)
        {
            var characterUID = bag.PreviousOwnerUID;

            if (!bag.IsStashBag())
            {
                Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Bag '{bag.Name}' with ItemID {bag.ItemID} is not a StashPack.");
                return;
            }

            if (bag.OwnerCharacter != null)
            {
                Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Bag '{bag.Name}' is currently owned by Character " +
                    $"UID {bag.OwnerCharacter.UID}. Bag is likely equipped or in character's inventory. Not updating stash.");
                return;
            }

            Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Bag.PreviousOwnerUID = '{characterUID}'");
            if (!_stashBagState.IsAvailableForSyncing(characterUID, bag.UID))
            {
                Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Ignoring container content change event. Bag '{bag.Name}' ({bag.UID}) is currently not available for syncing.");
                return;
            }
            if (!_instanceFactory.TryGetStashPackWorldData(out var stashPackWorldData))
            {
                Logger.LogWarning($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Could not get a {nameof(StashPackWorldData)} instance for " +
                    $"Bag '{bag.Name}' owned by Character UID '{characterUID}'. Unexpected. Expected an instance to be available by the time a bag's contents" +
                    $"can be changed. '{bag.Name}' Bag's content changes will not be saved to Stash!");
                return;
            }
            Logger.LogTrace($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Got {nameof(StashPackWorldData)} instance.");
            var stashPack = stashPackWorldData.GetStashPack(bag.UID);
            if (stashPack == null)
            {
                Logger.LogWarning($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Could not get a {nameof(StashPack)} instance for " +
                    $"Bag '{bag.Name}' owned by Character UID '{characterUID}'. '{bag.Name}' Bag's content changes will not be saved to Stash!");
                return;
            }

            Logger.LogTrace($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Got {nameof(StashPack)} instance for area '{stashPack.HomeArea.GetName()}'.");
            if (!_instanceFactory.TryGetStashSaveData(characterUID, out var stashSaveData))
            {
                Logger.LogError($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Error:" +
                    $"Could not get a {nameof(StashSaveData)} instance for Character UID '{characterUID}'. '{bag.Name}' Bag's content changes will not be saved to Stash!");
                return;
            }
            Logger.LogTrace($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Got {nameof(StashSaveData)} instance for character UID '{characterUID}'.");
            
            var stashSaveExecuter = _instanceFactory.GetEventStashSaveExecuter(characterUID, stashSaveData);

            Logger.LogTrace($"{nameof(StashPackBehaviors)}::{nameof(ContentsChanged)}: Queuing plan execution for next save event for character '{characterUID}' and bag {bag.Name} ({bag.UID}).");
            stashSaveExecuter.ExecutePlan(stashPack.HomeArea,
                () => GetSyncPlan(stashPack, stashPackWorldData, stashSaveData),
                (syncPlan) => _syncPlanner.LogSyncPlan(syncPlan)
                );
        }
        private ContainerSyncPlan GetSyncPlan(StashPack stashPack, StashPackWorldData stashPackData, StashSaveData stashSaveData)
        {
            var stashSave = stashSaveData.GetStashSave(stashPack.HomeArea);
            if (stashSave == null)
            {
                Logger.LogInfo($"Could not update {stashPack.HomeArea.GetName()} stash for Character UID '{stashPack.StashBag.OwnerCharacter.UID}'." +
                    $" This may be because a save for this area does not exist yet.");
                return null;
            }
            var newStashItemSaves = stashPack.StashBag.Container.GetContainedItems().Select(i => 
                (new BasicSaveData(i.UID, i.ToSaveData()))
                .ToUpdatedHierachy(stashSave.UID, stashSave.ItemID)
            );

            return _syncPlanner.PlanSync(stashSave, stashPack.StashBag.ContainedSilver, newStashItemSaves);
        }
        private void BeingEquippedOrTaken(Bag bag, EquipmentSlot slot)
        {
            Logger.LogDebug($"{nameof(StashPackBehaviors)}::{nameof(BeingEquippedOrTaken)}: triggered for Bag {bag.Name}.");
            if (bag != null && bag.IsStashBag())
                bag.EmptyContents();
        }
        #endregion
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
