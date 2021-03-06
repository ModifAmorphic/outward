using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal class BagPickedActions : MajorBagActions
    {
        private ConcurrentDictionary<string, byte> _takingBags = new ConcurrentDictionary<string, byte>();

        public BagPickedActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public override void SubscribeToEvents()
        {
            //ItemEvents.PerformEquipBefore += BagPickedUp;
            CharacterEvents.HandleBackpackBefore += TryHandleBackpackBefore;
            CharacterInventoryEvents.GetMostRelevantContainerAfter += GetMostRelevantContainer;
            SceneManager.sceneLoaded += (s, l) => _takingBags = new ConcurrentDictionary<string, byte>();
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
                var bag = character.Interactable?.ItemToPreview as Bag;
                if (bag != null && _takingBags.ContainsKey(bag.UID))
                {
                    Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(TryHandleBackpackBefore)}: Bag {bag.Name} ({bag.UID}) is already being picked up.");
                    return false;
                }
                if (!IsCurrentSceneStashPackEnabled())
                {
                    
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
            string bagUID = bag.UID;

            if (_takingBags.ContainsKey(bagUID))
            {
                Logger.LogTrace($"{nameof(BagPickedActions)}::{nameof(TakeBag)}: Bag {bag.Name} ({bag.UID}) is already being taken.");
                return true;
            }

            _ = _takingBags.TryAdd(bagUID, new byte());
            Logger.LogTrace($"{nameof(BagPickedActions)}::{nameof(TakeBag)}: Marked Bag {bag.Name} ({bag.UID}) as being taken.");

            if (!_instances.TryGetItemManager(out var itemManager))
            {
                Logger.LogError($"{nameof(BagPickedActions)}::{nameof(TakeBag)}: Unable to get ItemManager. Special pickup processing canceled for {bag.Name} ({bag.UID}).");
                _takingBags.TryRemove(bagUID, out var _);
                return false;
            }
            var bagItemID = bag.ItemID;
            DoAfterBagTracked(bag, _instances.GetBagStateService(character.UID),
                () => DestroyBag(bag));

            DoAfterBagDestroyed(bagUID, () =>
            {
                var bagPrefab = ResourcesPrefabManager.Instance.GetItemPrefab(bagItemID);
                character.Inventory.GenerateItem(bagPrefab, 1, false);
                character.Inventory.NotifyItemTake(bagPrefab, 1);
                _takingBags.TryRemove(bagUID, out var _);
            });
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
            Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}:  '{charUID}' Bag {bag.Name} ({bag.UID}) is being picked up.");

            var bagStates = _instances.GetBagStateService(charUID);
            if (!character.IsLocalPlayer)
            {
                Logger.LogDebug($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: Player Character '{charUID}' is not a local player. Ignoring Pick Up event for bag '{bag.Name}' ({bag.UID}) " +
                    $"and disabling.");
                BagStateService.DisableBag(bag.UID);
                return true;
            }

            bagStates.DisableContentChangeTracking(bag.ItemID);

            //if (!bagStates.TryGetState(bag.ItemID, out var bagState) && bag.HasContents())
            //{
            //    if (bag.PreviousOwnerUID == charUID)
            //    {
            //        Logger.LogWarning($"{nameof(BagPickedActions)}::{nameof(HandleBackpackBefore)}: No existing state found for bag, but already has contents. Disabling Stashpack functionality.");
            //        BagStateService.DisableBag(bag.UID);
            //        return true;
            //    }
            //}

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
