using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents
{
    internal class BagDropActions : MajorBagActions
    {
        public BagDropActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public void SubscribeToEvents()
        {
            //CharacterInventoryEvents.DropBagItemBefore += DropBagItemBefore;
            CharacterInventoryEvents.DropBagItemAfter += DropBagItemAfter;
        }

        private void DropBagItemBefore(Character character, Bag bag)
        {
            throw new NotImplementedException();
        }

        private void DropBagItemAfter(Character character, Bag bag)
        {
            string charUID = character.UID.ToString();
            Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemAfter)}: Character '{charUID}' dropped Bag {bag.Name} ({bag.UID}).");
            var bagStates = _instances.GetBagStateService(charUID);

            if (bagStates.IsBagDisabled(bag.UID))
            {
                Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemAfter)}: Bag {bag.Name} ({bag.UID}) has StashBag functionality disabled.");
                return;
            }
            if (IsHost(character) && GetBagAreaEnum(bag) == GetCurrentAreaEnum())
            {
                Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemAfter)}: Character '{charUID}' is hosting the game and is in '{bag.Name}' ({bag.UID}) home area {GetBagAreaEnum(bag).GetName()}. StashBag functionalty disabled for bag.");
                bagStates.DisableBag(bag.UID);
                return;
            }

            bagStates.DisableTracking(bag.ItemID);

            UnclaimClearOtherBags(charUID, bag);

            if (!TryRestoreState(charUID, bag))
            {
                if (!TryRestoreStash(charUID, bag))
                {
                    Logger.LogInfo($"Unable restore either state or stash to bag. Disabling StashPack functionality and treating as a regular backback.");
                    bagStates.DisableBag(bag.UID);
                    return;
                }
            }

            DoAfterBagLoaded(bag, () => SaveStateEnableTracking(charUID, bag));
        }

        private void SaveStateEnableTracking(string characterUID, Bag bag)
        {
            var bagStates = _instances.GetBagStateService(characterUID);
            if (!bagStates.TrySaveState(bag))
            {
                Logger.LogWarning($"{nameof(BagDropActions)}::{nameof(DropBagItemAfter)}: Unable to save state for character '{characterUID}' bag '{bag.Name}' ({bag.UID}). StashBag functionalty may not be working for this bag.");
                bagStates.DisableBag(bag.UID);
                return;
            }
            bagStates.SetSyncedFromStash(bag.ItemID, true);
            bagStates.EnableTracking(bag.ItemID);
        }

        private void UnclaimClearOtherBags(string characterUID, Bag claimedBag)
        {
            if (_instances.TryGetStashPackWorldData(out var stashPackWorldData))
            {
                var packs = stashPackWorldData.GetStashPacks(characterUID);
                if (packs != null)
                {
                    var bagsToFree = packs.Where(p => p.StashBag.ItemID == claimedBag.ItemID && p.StashBag.UID != claimedBag.UID).Select(p => p.StashBag);
                    foreach (var bag in bagsToFree)
                    {
                        bag.PreviousOwnerUID = string.Empty;
                        bag.EmptyContents();
                        Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(UnclaimClearOtherBags)}: Removed character's ({characterUID}) claim from bag {bag.Name} ({bag.UID}) and emptied its contents.");
                    }
                }
            }
        }

        private bool TryRestoreState(string characterUID, Bag bag)
        {
            var bagStates = _instances.GetBagStateService(characterUID);
            if (bagStates.TryGetState(bag.ItemID, out var bagState) && _instances.TryGetItemManager(out var itemManager))
            {
                var silver = bagState.BasicSaveData.GetContainerSilver();
                var updatedBagSilver = (new BasicSaveData(bag.UID, bag.ToSaveData())).ToUpdatedContainerSilver(silver);

                var loadItems = bagState.ItemsSaveData.Select(isd =>
                    isd.ToUpdatedHierachy(bag.UID + StashPacksConstants.BagUidSuffix, bag.Container.ItemID))
                    .ToList();
                loadItems.Add(updatedBagSilver);
                itemManager.LoadItems(loadItems);
                Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(TryRestoreState)}: Removed bag {bag.Name} ({bag.UID}) state for character ({characterUID})." +
                    $" Set silver to {silver} and loaded {bagState.ItemsSaveData.Count()} items.");
                return true;
            }
            Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(TryRestoreState)}: No state found for bag {bag.Name} ({bag.UID}) owned by character ({characterUID}).");
            return false;
        }

        private bool TryRestoreStash(string characterUID, Bag bag)
        {
            if (_instances.TryGetStashSaveData(characterUID, out var stashSaveData))
            {
                var stashSave = stashSaveData.GetStashSave(GetBagAreaEnum(bag));
                if (stashSave != null)
                {
                    var bagSave = bag.ToBagState(characterUID, _instances.AreaStashPackItemIds);
                    var stashItems = stashSave.ItemsSaveData.Select(i => i.ToUpdatedHierachy(bag.UID + StashPacksConstants.BagUidSuffix, bag.Container.ItemID));
                    var syncPlanner = _instances.GetSyncPlanner();

                    var syncPlan = syncPlanner.PlanSync(bagSave, stashSave.BasicSaveData.GetContainerSilver(), stashItems);
                    syncPlanner.LogSyncPlan(syncPlan);

                    if (_instances.TryGetStashPackWorldExecuter(out var planExecuter))
                    {
                        Logger.LogInfo($"Executing sync plan. Restoring {GetBagAreaEnum(bag).GetName()} Stash to  {bag.Name} ({bag.UID}) owned by character ({characterUID}).");
                        planExecuter.ExecutePlan(syncPlan);
                        return true;
                    }
                }
            }
            Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(TryRestoreStash)}: Unable to restore {GetBagAreaEnum(bag).GetName()} Stash to bag {bag.Name} ({bag.UID}) owned by character ({characterUID}).");
            return false;
        }
    }
}
