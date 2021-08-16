using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Reflection;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal class BagPickedActions : MajorBagActions
    {

        public BagPickedActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public override void SubscribeToEvents()
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
                    if (!bag.HasContents())
                    {
                        character.Inventory.TakeItem(bag, false);
                        bag.Container.AllowOverCapacity = false;
                        return false;
                    }
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

            if (!bagStates.TryGetState(bag.ItemID, out var bagState) && bag.HasContents())
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: No existing state found for bag, but already has contents. Disabling Stashpack functionality.");
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

            if (BagStateService.IsBagDisabled(bag.UID) && !DisableHostBagIfInHomeArea(character, bag))
                BagStateService.EnableBag(bag.UID);

            character.Inventory.TakeItem(bag, false);
            bag.Container.AllowOverCapacity = false;

            return false;
        }
    }
}
