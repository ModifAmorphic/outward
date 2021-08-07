using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.Settings;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents
{
    internal class BagDropActions : MajorBagActions
    {
        public BagDropActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public void SubscribeToEvents()
        {
            CharacterInventoryEvents.DropBagItemBefore += DropBagItemBefore;
            CharacterInventoryEvents.DropBagItemAfter += DropBagItemAfter;
        }

        private void DropBagItemBefore(Character character, Bag bag)
        {
            if (BagStateService.IsBagDisabled(bag.UID))
            {
                Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemBefore)}: Bag {bag.Name} ({bag.UID}) has StashBag functionality disabled. Not scaling.");
                return;
            }

            BagVisualizer.ScaleBag(bag);
        }

        private void DropBagItemAfter(Character character, Bag bag)
        {
            if (BagStateService.IsBagDisabled(bag.UID))
            {
                Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemAfter)}: Bag {bag.Name} ({bag.UID}) has StashBag functionality disabled.");
                return;
            }

            BagVisualizer.BagDropping(bag, character.transform);

            DoAfterBagLoaded(bag, () => ProcessBagDropAfterLoaded(character, bag));
        }
        private void ProcessBagDropAfterLoaded(Character character, Bag bag)
        {
            string charUID = character.UID.ToString();
            Logger.LogDebug($"{nameof(BagDropActions)}::{nameof(DropBagItemAfter)}: Character '{charUID}' dropped Bag {bag.Name} ({bag.UID}).");
            var bagStates = _instances.GetBagStateService(charUID);

            DisableHostBagIfInHomeArea(character, bag);

            bagStates.DisableTracking(bag.ItemID);

            UnclaimClearOtherBags(charUID, bag);

            if (!bag.Container.AllowOverCapacity)
                bag.Container.AllowOverCapacity = true;

            if (!TryRestoreState(charUID, bag))
            {
                if (!TryRestoreStash(charUID, bag))
                {
                    Logger.LogInfo($"Unable restore either state or stash to bag. Disabling StashPack functionality and treating as a regular backback.");
                    BagStateService.DisableBag(bag.UID);
                    return;
                }
            }


            DoAfterBagLoaded(bag, () => SaveStateEnableTracking(charUID, bag.UID));
            DoAfterBagLanded(bag, () => BagVisualizer.BagLanded(bag, character.transform));
        }

        private void DoAfterBagLanded(Bag bag, Action invokeAfter)
        {
            _instances.UnityPlugin.StartCoroutine(AfterBagLandedCoroutine(bag, invokeAfter));
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
    }
}
