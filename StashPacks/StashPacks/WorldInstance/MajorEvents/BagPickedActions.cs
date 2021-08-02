using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents
{
    internal class BagPickedActions
    {
        private readonly InstanceFactory _instances;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public BagPickedActions(InstanceFactory instances, Func<IModifLogger> getLogger) => (_instances, _getLogger) = (instances, getLogger);

        public void SubscribeToEvents()
        {
            ItemEvents.PerformEquipBefore += BagPickedUp;
        }

        private void BagPickedUp(Bag bag, EquipmentSlot equipmentSlot)
        {
            string charUID = equipmentSlot.Character.UID.ToString();
            Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Character '{charUID}' Bag {bag.Name} ({bag.UID}) is being picked up.");

            var bagStates = _instances.GetBagStateService(charUID);
            if (!equipmentSlot.Character.IsLocalPlayer)
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Player Character '{charUID}' is not a local player. Ignoring Pick Up event for bag '{bag.Name}' ({bag.UID}) " +
                    $"and disabling.");
                BagStateService.DisableBag(bag.UID);
                return;
            }
            bagStates.DisableTracking(bag.ItemID);

            if (!bag.HasContents())
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Bag {bag.Name} ({bag.UID}) is empty.");
                return;
            }
            if (!bagStates.TryGetState(bag.ItemID, out var bagState) && bag.HasContents())
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: No existing state found for bag, but already has contents. Disabling and treating as a regular backpack bag.");
                BagStateService.DisableBag(bag.UID);
                return;
            }

            bag.EmptyContents();
            Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Character '{charUID}' Bag {bag.Name} ({bag.UID}) contents cleared on pickup.");
        }
    }
}
