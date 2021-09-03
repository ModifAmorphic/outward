using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal class BagDropActions : MajorBagActions
    {
        public BagDropActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public override void SubscribeToEvents()
        {
            CharacterInventoryEvents.DropBagItemBefore += (character, bag) =>
            {
                if (PhotonNetwork.isNonMasterClientInRoom)
                {
                    _instances.StashPackNet.SendDroppingStashPack(character.UID, bag.UID);
                }
            };
            CharacterInventoryEvents.DropBagItemAfter += DropBagItemAfter;
        }

        //private void DropBagItemBefore(Character character, Bag bag)
        //{
        //    if (!IsCurrentSceneStashPackEnabled())
        //    {
        //        Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemBefore)}: Current Scene is not StashPack Enabled. Disabling stashpack functionality for bag {bag.Name} ({bag.UID}).");
        //        BagStateService.DisableBag(bag.UID);
        //        return;
        //    }

        //    if (BagStateService.IsBagDisabled(bag.UID) || DisableHostBagIfInHomeArea(character, bag))
        //    {
        //        Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemBefore)}: Bag {bag.Name} ({bag.UID}) has StashBag functionality disabled. Not scaling.");
        //        return;
        //    }
        //}

        private void DropBagItemAfter(Character character, Bag bag)
        {
            if (BagStateService.IsBagDisabled(bag.UID))
            {
                Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemAfter)}: Bag {bag.Name} ({bag.UID}) has StashBag functionality disabled.");
                UnclaimAndClearBag(bag);
                return;
            }

            DoAfterBagLoaded(bag.UID, (b) => ProcessBagDropAfterLoaded(character, b));
        }
        private void ProcessBagDropAfterLoaded(Character character, Bag bag)
        {
            string charUID = character.UID.ToString();
            Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemAfter)}: Character '{charUID}' dropped Bag {bag.Name} ({bag.UID}).");
            var bagStates = _instances.GetBagStateService(charUID);

            if (DisableHostBagIfInHomeArea(character, bag))
            {
                return;
            }

            bagStates.DisableContentChangeTracking(bag.ItemID);

            UnclaimClearOtherBags(charUID, bag);

            if (!bag.Container.AllowOverCapacity)
            {
                bag.Container.AllowOverCapacity = true;
            }

            if (!TryRestoreState(charUID, bag))
            {
                if (!TryRestoreStash(charUID, bag))
                {
                    Logger.LogInfo($"Unable restore either state or stash to bag. Disabling StashPack functionality.");
                    UnclaimAndClearBag(bag);
                    return;
                }
            }
            bag.Container.SpecialType = ItemContainer.SpecialContainerTypes.Stash;
            DoAfterBagLoaded(bag.UID, (Bag b) => SaveStateEnableTracking(charUID, b.UID));
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
                    if (_instances.TryGetStashPackWorldExecuter(out var planExecuter))
                    {
                        Logger.LogInfo($"Executing sync plan. Restoring {GetBagAreaEnum(bag).GetName()} Stash to {bag.Name} ({bag.UID}) owned by character ({characterUID}).");
                        syncPlanner.LogSyncPlan(syncPlan);
                        planExecuter.ExecutePlan(syncPlan);
                        return true;
                    }
                }
            }
            Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(TryRestoreStash)}: Unable to restore {GetBagAreaEnum(bag).GetName()} Stash to bag {bag.Name} ({bag.UID}) owned by character ({characterUID}).");
            return false;
        }
        private bool TryRestoreState(string characterUID, Bag bag)
        {
            var bagStates = _instances.GetBagStateService(characterUID);
            if (bagStates.TryGetState(bag.ItemID, out var bagState) && _instances.TryGetItemManager(out var itemManager))
            {
                var bagSave = bag.ToBagState(characterUID, _instances.AreaStashPackItemIds);
                var stateItems = bagState.ItemsSaveData.Select(i => i.ToUpdatedHierachy(bag.UID + StashPacksConstants.BagUidSuffix, bag.Container.ItemID));
                var syncPlanner = _instances.GetSyncPlanner();

                var syncPlan = syncPlanner.PlanSync(bagSave, bagState.BasicSaveData.GetContainerSilver(), stateItems);

                if (_instances.TryGetStashPackWorldExecuter(out var planExecuter))
                {
                    Logger.LogInfo($"Executing sync plan. Restoring {GetBagAreaEnum(bag).GetName()} State to {bag.Name} ({bag.UID}) owned by character ({characterUID}).");
                    syncPlanner.LogSyncPlan(syncPlan);
                    planExecuter.ExecutePlan(syncPlan);
                    return true;
                }
            }
            Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(TryRestoreState)}: No state found for bag {bag.Name} ({bag.UID}) owned by character ({characterUID}).");
            return false;
        }
    }
}
