using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Linq;
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
            CharacterInventoryEvents.GetMostRelevantContainerAfter += GetMostRelevantContainer;
        }

        private ItemContainer GetMostRelevantContainer(Bag bag, ItemContainer pouchContainer, ItemContainer defaultContainer)
        {
            if (_instances.StashPacksSettings.PreferPickupToPouch.Value)
            {
                return pouchContainer;
            }

            return defaultContainer;
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
                        UnclaimAndClearBag(bag);
                    }
                    if (!bag.HasContents())
                    {
                        return !TakeBag(character, bag);
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
        private bool TakeBag(Character character, Bag bag)
        {
            if (!_instances.TryGetItemManager(out var itemManager))
            {
                Logger.LogError($"{nameof(BagPickedActions)}::{nameof(TakeBag)}: Unable to get ItemManager. Special pickup processing canceled for {bag.Name} ({bag.UID}).");
                return false;
            }
            if (bag.HasContents())
            {
                var bagContents = bag.Container.GetContainedItems().ToList();
                foreach (var item in bagContents)
                {
                    if (!PhotonNetwork.isNonMasterClientInRoom)
                    {
                        itemManager.DestroyItem(item.UID);
                    }
                    else
                    {
                        itemManager.SendDestroyItem(item.UID);
                    }
                }
                bag.Container.RemoveAllSilver();
            }

            string bagUID = bag.UID;
            int bagItemID = bag.ItemID;
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(TakeBag)}: Destroying Bag {bag.Name} ({bag.UID}).");
                itemManager.DestroyItem(bag.UID);
            }
            else
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(TakeBag)}: Sending Request to Destroy Bag {bag.Name} ({bag.UID}).");
                itemManager.SendDestroyItem(bag.UID);
            }

            _instances.UnityPlugin.StartCoroutine(AfterBagDestroyedCoroutine(bagUID, () =>
            {
                var bagPrefab = ResourcesPrefabManager.Instance.GetItemPrefab(bagItemID);
                character.Inventory.GenerateItem(bagPrefab, 1, false);
                character.Inventory.NotifyItemTake(bagPrefab, 1);
            }));
            _instances.StashPackNet.SendStashPackLinkChanged(bagUID, character.UID, false);
            return true;
        }
        private bool HandleBackpackBefore(Character character, CharacterPrivates cprivates)
        {
            var canInteract = typeof(Character).GetMethod("CanInteract", BindingFlags.NonPublic | BindingFlags.Instance);

            if (Time.time - (double)cprivates.m_lastHandleBagTime < 1.0
                || !((bool)canInteract.Invoke(character, null))
                || (cprivates.m_interactCoroutinePending || cprivates.m_IsHoldingInteract)
                || (cprivates.m_IsHoldingDragCorpse || character.CharacterUI.IsDialogueInProgress))
            {
                return true;
            }

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
            bagStates.DisableContentChangeTracking(bag.ItemID);

            if (!bagStates.TryGetState(bag.ItemID, out var bagState) && bag.HasContents())
            {
                if (bag.PreviousOwnerUID == charUID)
                {
                    Logger.LogWarning($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: No existing state found for bag, but already has contents. Disabling Stashpack functionality.");
                    BagStateService.DisableBag(bag.UID);
                    return true;
                }
            }

            Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: Placing Bag {bag.Name} ({bag.UID}) into Character '{charUID}' inventory. character.transform: {character.transform?.name}," +
                $" character.transform.parent: {character.transform?.parent?.name}");

            if (BagStateService.IsBagDisabled(bag.UID) && !DisableHostBagIfInHomeArea(character, bag))
            {
                BagStateService.EnableBag(bag.UID);
            }

            return !TakeBag(character, bag);
        }
    }
}
