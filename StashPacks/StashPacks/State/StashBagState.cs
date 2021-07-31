using BepInEx;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.WorldInstance.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.State
{
    internal class StashBagState
    {
        private readonly InstanceFactory _instanceFactory;
        /// <summary>
        /// <see cref="ConcurrentDictionary{TKey, TValue}"/> TKey = Bag UID. TValue not used.
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _bagSyncFlag = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, StashBag>> _stashPackStates = new ConcurrentDictionary<string, ConcurrentDictionary<int, StashBag>>();
        //private readonly ConcurrentDictionary<string, StashPackSave> _stashPackStates = new ConcurrentDictionary<string, StashPackSave>();

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public StashBagState(InstanceFactory instanceFactory, Func<IModifLogger> getLogger)
        {
            (_instanceFactory, _getLogger) = (instanceFactory, getLogger);
            RouteEvents();
        }
        private void RouteEvents()
        {
            BagEvents.BagDropCastBefore += bag => ProcessDropBagItemBefore(bag.OwnerCharacter.UID.ToString(), ref bag);
            CharacterInventoryEvents.DropBagItemBefore += (character, bag) => ProcessDropBagItemBefore(character.UID.ToString(), ref bag);
            ItemEvents.PerformEquipBefore += ProcessEquipBefore;

            //this might be ok now. Original intent was to make sure everything was cleared on a level reload,
            //which may still be needed.  But, bag sync is disabled while applying data now so this is actually necessary now.
            //Review for save instance reset scenario.
            EnvironmentSaveEvents.ApplyDataAfter += envSave =>
                _instanceFactory.UnityPlugin.StartCoroutine(ClearStatesAfterItemsLoaded());

        }
        private void ProcessDropBagItemBefore(string characterUID, ref Bag bag)
        {
            if (bag == null || !bag.IsStashBag())
                return;
            
            string bagUID = bag.UID;
            if (TryRemoveStashBag(characterUID, bag.ItemID, out var stashBag))
            {
                if (_instanceFactory.TryGetItemManager(out var itemManager))
                {
                    DisableSyncing(characterUID, bagUID);
                    Logger.LogDebug($"{nameof(StashBagState)}::{nameof(ProcessDropBagItemBefore)}:" +
                        $" Restoring bag '{bag.Name}' ({bagUID}) from state. Setting silver to {stashBag.BasicSaveData.GetContainerSilver()}." +
                        $" Loading {stashBag.ItemsSaveData.Count()} items.");
                    bag.Container.SetSilverCount(stashBag.BasicSaveData.GetContainerSilver());
                    
                    var parentItemId = bag.ItemID;
                    //if (stashBag.ItemsSaveData.Any())
                    //    parentItemId = stashBag.ItemsSaveData.FirstOrDefault().GetHierarchyData().ParentItemID;

                    itemManager.LoadItems(stashBag.ItemsSaveData.Select(i => i.ToUpdatedHierachy(bagUID + StashPacksConstants.BagUidSuffix, parentItemId)).ToList());
                    _instanceFactory.UnityPlugin.StartCoroutine(ClearStateEnableSyncAfterItemsLoaded(characterUID, bag.ItemID, bagUID));
                }
            }
        }
        private void ProcessEquipBefore(Bag bag, EquipmentSlot slot)
        {
            if (bag == null || !bag.IsStashBag())
                return;
            string charUID = slot.Character.UID.ToString();

            var stashBag = bag.ToStashBag(StashPacksConstants.StashBackpackItemIds);
            //ToStashBag() uses Previous over current Character UIDs, so set to new owner instead.
            stashBag.CharacterUID = charUID;
            DisableSyncing(charUID, bag.UID);

            //If picked up by a new owner, save state for previous owner then clear out items for new owner.
            if (bag.PreviousOwnerUID != charUID && !string.IsNullOrEmpty(bag.PreviousOwnerUID))
            {
                RemoveSyncTracking(bag.PreviousOwnerUID, bag.UID);
                Logger.LogDebug($"{nameof(StashBagState)}::{nameof(ProcessEquipBefore)}:" +
                        $" Equipping Character '{charUID}' Differs from Previous Character 'bag.PreviousOwnerUID' for '{bag.Name}' ({bag.ItemID}). Saving other character's bag state and clearing bag contents.");
                var otherBag = bag.ToStashBag(StashPacksConstants.StashBackpackItemIds);
                AddOrUpdateState(bag.PreviousOwnerUID, bag.ItemID, otherBag);

                stashBag.BasicSaveData = stashBag.BasicSaveData.ToUpdatedContainerSilver(0);
                stashBag.ItemsSaveData = new List<BasicSaveData>();
            }
            
            AddOrUpdateState(charUID, bag.ItemID, stashBag);
        }
        private void AddOrUpdateState(string characterUID, int bagItemId, StashBag stashBag)
        {
            if (!_stashPackStates.TryGetValue(characterUID, out var bagStates))
            {
                bagStates = new ConcurrentDictionary<int, StashBag>();
                _stashPackStates.TryAdd(characterUID, bagStates);
            }

            bagStates.AddOrUpdate(stashBag.ItemID, stashBag, (k, v) =>
            {
                Logger.LogDebug($"Merging existing bag {stashBag.ItemID} state for character '{characterUID}'.");
                var silver1 = v.BasicSaveData.GetContainerSilver();
                var silver2 = stashBag.BasicSaveData.GetContainerSilver();
                var mergedItems = v.ItemsSaveData.ToList();
                mergedItems.AddRange(stashBag.ItemsSaveData);
                stashBag.ItemsSaveData = mergedItems.ToList();
                stashBag.BasicSaveData = v.BasicSaveData.ToUpdatedContainerSilver(silver1 + silver2);
                return stashBag;
            });

        }
        public void EnableSyncing(string characterUID, string bagUid)
        {
            var bags = _bagSyncFlag.GetOrAdd(characterUID, new ConcurrentDictionary<string, bool>());
            _ = bags.AddOrUpdate(bagUid, true, (k, v) => true);
        }
        public void DisableSyncing(string characterUID, string bagUid)
        {
            var bags = _bagSyncFlag.GetOrAdd(characterUID, new ConcurrentDictionary<string, bool>());
            _ = bags.AddOrUpdate(bagUid, false, (k, v) => false);

            //if (TryGetStashBag(characterUID, bagItemId, out var stashBag))
            //{
            //    stashBag.SyncDisabled = true;
            //    Logger.LogDebug($"{nameof(StashBagState)}::{nameof(DisableSyncing)}:" +
            //        $" Disabled Syncing for Character '{characterUID}', Bag ItemID {bagItemId}.");
            //}
        }
        public void RemoveSyncTracking(string characterUID, string bagUID)
        {
            if (_bagSyncFlag.TryGetValue(characterUID, out var bags))
                _ = bags.TryRemove(bagUID, out _);
        }
        public bool TryGetStashBag(string characterUID, int bagItemId, out StashBag stashBag)
        {
            if (_stashPackStates.TryGetValue(characterUID, out var stashBags))
            {
                if (stashBags.TryGetValue(bagItemId, out stashBag))
                {
                    return true;
                }
            }
            stashBag = null;
            return false;
        }
        public bool TryRemoveStashBag(string characterUID, int bagItemId, out StashBag stashBag)
        {
            if (_stashPackStates.TryGetValue(characterUID, out var stashBags))
            {
                return stashBags.TryRemove(bagItemId, out stashBag);
            }
            stashBag = null;
            return false;
        }
        public bool IsAvailableForSyncing(string characterUID, string bagUID)
        {
            var bagUids = _bagSyncFlag.GetOrAdd(characterUID, new ConcurrentDictionary<string, bool>());
            return bagUids.GetOrAdd(bagUID, true);


            //if (TryGetStashBag(characterUID, bagItemId, out var stashBag))
            //{
            //    Logger.LogDebug($"{nameof(StashBagState)}::{nameof(IsAvailableForSyncing)}:" +
            //        $" {nameof(stashBag.SyncDisabled)} = {stashBag.SyncDisabled} for Character '{characterUID}', Bag ItemID {bagItemId}.");
            //    return !stashBag.SyncDisabled;
            //}
            //Logger.LogDebug($"{nameof(StashBagState)}::{nameof(IsAvailableForSyncing)}:" +
            //        $" Could not find existing state for Character '{characterUID}', Bag ItemID {bagItemId}. Assuming true.");
            //return true;
        }
        /// <summary>
        /// Clears states of for bag UID matching <paramref name="bagUID"/> who do not belong to the <paramref name="ownerCharacterUID"/>.
        /// </summary>
        /// <param name="ownerCharacterUID">The character UID who's bag state should not be cleared.</param>
        /// <param name="bagUID">The UID of the bag.</param>
        /// <returns>true if any bags with a matching <paramref name="bagUID"/> were cleared.  Otherwise false.</returns>
        public bool ClearOtherCharactersStates(string ownerCharacterUID, int bagItemId)
        {
            var clearedBags = false;
            var otherBags = _stashPackStates.Where(kvp => kvp.Key != ownerCharacterUID && kvp.Value.ContainsKey(bagItemId)).Select(b => new { 
                b.Key,
                bagItemId
            }).ToList();

            foreach (var other in otherBags)
            {
                _stashPackStates[other.Key].TryRemove(other.bagItemId, out _);
                clearedBags = true;
            }
            return clearedBags;
        }
        public IEnumerator ClearStateEnableSyncAfterItemsLoaded(string characterUID, int bagItemId, string bagUID)
        {
            if (_instanceFactory.TryGetItemManager(out var itemManager))
            {
                var bag = itemManager.GetItem(bagUID);

                while (!itemManager.IsAllItemSynced || bag == null || string.IsNullOrWhiteSpace(bag.PreviousOwnerUID))
                {
                    bag = itemManager.GetItem(bagUID);
                    Logger.LogTrace($"{nameof(StashBagState)}::{nameof(ClearStateEnableSyncAfterItemsLoaded)}: Trying to clear state for character '{characterUID}' bag '{bagItemId}'.  itemManager.IsAllItemSynced = {itemManager.IsAllItemSynced}.");
                    yield return new WaitForSeconds(.2f);
                }

                Logger.LogDebug($"{nameof(StashBagState)}::{nameof(ClearStateEnableSyncAfterItemsLoaded)}: Clearing states for character '{characterUID}' bag '{bagItemId}'.");
                TryRemoveStashBag(characterUID, bagItemId, out _);
                EnableSyncing(characterUID, bagUID);
            }
        }
        public IEnumerator ClearStatesAfterItemsLoaded()
        {
            while (!_instanceFactory.TryGetItemManager(out var itemManager) || !itemManager.IsAllItemSynced)
                yield return new WaitForSeconds(.2f);

            Logger.LogDebug($"{nameof(StashBagState)}::{nameof(ClearStatesAfterItemsLoaded)}: Clearing all states and sync flags for all bags.");
            _stashPackStates.Clear();
            _bagSyncFlag.Clear();
        }
    }
}
