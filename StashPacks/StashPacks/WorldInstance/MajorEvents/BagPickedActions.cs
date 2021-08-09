using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Reflection;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents
{
    internal class BagPickedActions : MajorBagActions
    {

        public BagPickedActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public void SubscribeToEvents()
        {
            //ItemEvents.PerformEquipBefore += BagPickedUp;
            CharacterEvents.HandleBackpackBefore += TryHandleBackpackBefore;
        }

        private bool TryHandleBackpackBefore(Character character, CharacterPrivates cprivates)
        {
            try
            {
                if (!IsCurrentSceneStashPackEnabled())
                {
                    var bag = character.Interactable?.ItemToPreview as Bag;
                    if (bag != null)
                    {
                        Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(TryHandleBackpackBefore)}: Current Scene is not StashPack Enabled. Disabling stashpack functionality for bag {bag.Name} ({bag.UID}).");
                        BagStateService.DisableBag(bag.UID);
                    }
                    return true;
                }
                return HandleBackpackBefore(character, cprivates);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(BagPickedActions)}::{nameof(TryHandleBackpackBefore)}: Failed to handle Backpack Event.", ex);
            }
            return true;
        }
        private bool HandleBackpackBefore(Character character, CharacterPrivates cprivates)
        {
            var canInteract = typeof(Character).GetMethod("CanInteract", BindingFlags.NonPublic | BindingFlags.Instance);

            if ((double)Time.time - (double)cprivates.m_lastHandleBagTime < 1.0 
                || !((bool)canInteract.Invoke(character, null)) 
                || (cprivates.m_interactCoroutinePending || cprivates.m_IsHoldingInteract) 
                || (cprivates.m_IsHoldingDragCorpse  || character.CharacterUI.IsDialogueInProgress))
                return true;
            var bag = character.Interactable?.ItemToPreview as Bag;
            if (bag == null)
            {
                Logger.LogWarning($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: Expected a bag.  Item being picked up by character '{character.UID}' is not a bag.");
                return true;
            }
            string charUID = character.UID.ToString();
            Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: Character '{charUID}' Bag {bag.Name} ({bag.UID}) is being picked up.");

            var bagStates = _instances.GetBagStateService(charUID);
            if (!character.IsLocalPlayer)
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: Player Character '{charUID}' is not a local player. Ignoring Pick Up event for bag '{bag.Name}' ({bag.UID}) " +
                    $"and disabling.");
                BagStateService.DisableBag(bag.UID);
                return true;
            }
            bagStates.DisableTracking(bag.ItemID);

            //if (!bag.HasContents())
            //{
            //    Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Bag {bag.Name} ({bag.UID}) is empty.");
            //    return true;
            //}

            if (!bagStates.TryGetState(bag.ItemID, out var bagState) && bag.HasContents())
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: No existing state found for bag, but already has contents. Disabling and treating as a regular backpack bag.");
                BagStateService.DisableBag(bag.UID);
                return true;
            }


            if (bag.HasContents())
            {
                bag.EmptyContents();
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: Character '{charUID}' Bag {bag.Name} ({bag.UID}) contents cleared on pickup.");
            }


            Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: Placing Bag {bag.Name} ({bag.UID}) into Character '{charUID}' inventory. character.transform: {character.transform?.name}," +
                $" character.transform.parent: {character.transform?.parent?.name}");

            character.Inventory.TakeItem(bag, false);
            bag.Container.AllowOverCapacity = false;

            return false;
        }

        private bool BagPickedUp(Bag bag, EquipmentSlot equipmentSlot)
        {
            string charUID = equipmentSlot.Character.UID.ToString();
            Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Character '{charUID}' Bag {bag.Name} ({bag.UID}) is being picked up.");

            var bagStates = _instances.GetBagStateService(charUID);
            if (!equipmentSlot.Character.IsLocalPlayer)
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Player Character '{charUID}' is not a local player. Ignoring Pick Up event for bag '{bag.Name}' ({bag.UID}) " +
                    $"and disabling.");
                BagStateService.DisableBag(bag.UID);
                return true;
            }
            bagStates.DisableTracking(bag.ItemID);

            if (!bag.HasContents())
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Bag {bag.Name} ({bag.UID}) is empty.");
                return true;
            }
            if (!bagStates.TryGetState(bag.ItemID, out var bagState) && bag.HasContents())
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: No existing state found for bag, but already has contents. Disabling and treating as a regular backpack bag.");
                BagStateService.DisableBag(bag.UID);
                return true;
            }
            
            bag.Container.AllowOverCapacity = false;
            bag.EmptyContents();
            equipmentSlot.Character.Inventory.TakeItem(bag, false);
            Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(BagPickedUp)}: Character '{charUID}' Bag {bag.Name} ({bag.UID}) contents cleared on pickup.");
            return false;
        }
    }
}
