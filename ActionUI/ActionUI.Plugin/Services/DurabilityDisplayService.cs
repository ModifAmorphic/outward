using ModifAmorphic.Outward.ActionUI.Extensions;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class DurabilityDisplayService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();
        private PlayerMenuService _playerMenuService;
        private bool _unequipedAdded;

        private bool _equipTracked = false;

        private bool _configured = false;

        private readonly static UnequippedDurabilityTracker UnequippedHelm = new UnequippedDurabilityTracker(EquipSlots.Head, DurableEquipmentType.Helm, 1f);
        private readonly static UnequippedDurabilityTracker UnequippedChest = new UnequippedDurabilityTracker(EquipSlots.Chest, DurableEquipmentType.Chest, 1f);
        private readonly static UnequippedDurabilityTracker UnequippedBoots = new UnequippedDurabilityTracker(EquipSlots.Feet, DurableEquipmentType.Boots, 1f);

        private readonly static Dictionary<EquipSlots, UnequippedDurabilityTracker> Unequipped = new Dictionary<EquipSlots, UnequippedDurabilityTracker>()
        {
            { UnequippedHelm.DurableEquipmentSlot, UnequippedHelm },
            { UnequippedChest.DurableEquipmentSlot, UnequippedChest },
            { UnequippedBoots.DurableEquipmentSlot, UnequippedBoots }
        };

        public DurabilityDisplayService(PlayerMenuService playerMenuService, Func<IModifLogger> loggerFactory)
        {
            (_playerMenuService, _loggerFactory) = (playerMenuService, loggerFactory);

            EquipmentPatches.AfterOnEquip += TrackEquippedItem;
            EquipmentPatches.AfterOnUnequip += UntrackEquippedItem;
            //SplitPlayerPatches.SetCharacterAfter += ConfigureShowHide;
            _playerMenuService.OnPlayerActionMenusConfigured += TryConfigureShowHide;
            SplitScreenManagerPatches.RemoveLocalPlayerAfter += SplitScreenManagerPatches_RemoveLocalPlayerAfter;
        }

        private void TryConfigureShowHide(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {
            try
            {
                ConfigureShowHide(actionMenus, splitPlayer);
            }
            catch (Exception ex)
            {
                Logger.LogException("Failed to configure DurabilityDisplayService.", ex);
            }
        }

        private void ConfigureShowHide(PlayerActionMenus actionMenus, SplitPlayer splitPlayer)
        {
            //var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            //var actionMenus = psp.GetService<PlayerActionMenus>();
            var profileService = actionMenus.ProfileManager.ProfileService;

            Reset(actionMenus);

            if (_configured)
                return;

            profileService.OnActiveProfileChanged += (profile) => ShowHide(splitPlayer.RewiredID, profile.DurabilityDisplayEnabled);
            profileService.OnActiveProfileSwitched += (profile) => ShowHide(splitPlayer.RewiredID, profile.DurabilityDisplayEnabled);

            if (actionMenus.DurabilityDisplay.IsAwake)
                ShowHide(splitPlayer.RewiredID, profileService.GetActiveProfile().DurabilityDisplayEnabled);
            else
                actionMenus.DurabilityDisplay.OnAwake.AddListener(() => ShowHide(splitPlayer.RewiredID, profileService.GetActiveProfile().DurabilityDisplayEnabled));

            _configured = true;
        }

        private void Reset(PlayerActionMenus actionMenus)
        {
            if (actionMenus.DurabilityDisplay.IsAwake)
            {
                Logger.LogDebug($"{nameof(DurabilityDisplayService)}::{nameof(Reset)}: Stopping Durability tracking for all slots for rewiredId={actionMenus.PlayerID}.");
                foreach (var slot in Enum.GetValues(typeof(EquipSlots)).Cast<EquipSlots>())
                    actionMenus.DurabilityDisplay.StopTracking(slot);
            }
        }

        private void ShowHide(int playerId, bool showDurabilityDisplay)
        {
            Logger.LogDebug($"{nameof(DurabilityDisplayService)}::{nameof(ShowHide)}: playerId={playerId}, showDurabilityDisplay={showDurabilityDisplay}");
            var psp = Psp.Instance.GetServicesProvider(playerId);
            var actionMenus = psp.GetService<PlayerActionMenus>();
            var display = actionMenus.DurabilityDisplay;
            if (showDurabilityDisplay)
            {
                if (!display.gameObject.activeSelf)
                {
                    display.gameObject.SetActive(true);
                }
                _equipTracked = true;
                //if (!_equipTracked)
                //{
                var splitPlayer = SplitScreenManager.Instance.LocalPlayers.FirstOrDefault(p => p.RewiredID == playerId);
                if (splitPlayer != null && splitPlayer?.AssignedCharacter != null)
                {

                    AddUnequippedTrackers(splitPlayer.RewiredID);
                    display.StopTracking(EquipSlots.LeftHand);
                    display.StopTracking(EquipSlots.RightHand);
                    TrackAllEquipment(splitPlayer.AssignedCharacter);
                }
                //}

            }
            else
            {
                if (display.gameObject.activeSelf)
                    display.gameObject.SetActive(false);
                if (_equipTracked)
                {
                    display.StopAllTracking();
                    _equipTracked = false;
                }
            }

        }

        private void TrackAllEquipment(Character character)
        {
            Logger.LogDebug($"{nameof(DurabilityDisplayService)}::{nameof(TrackAllEquipment)}: Player RewiredID {character.CharacterUI.RewiredID}");
            TrackEquipmentSlot(character, EquipmentSlot.EquipmentSlotIDs.Helmet);
            TrackEquipmentSlot(character, EquipmentSlot.EquipmentSlotIDs.Chest);
            TrackEquipmentSlot(character, EquipmentSlot.EquipmentSlotIDs.Foot);
            TrackEquipmentSlot(character, EquipmentSlot.EquipmentSlotIDs.RightHand);
            TrackEquipmentSlot(character, EquipmentSlot.EquipmentSlotIDs.LeftHand);
        }

        public void UntrackAllEquipment()
        {

        }

        private void TrackEquipmentSlot(Character character, EquipmentSlot.EquipmentSlotIDs slot)
        {
            if (!(character.Inventory.Equipment.IsEquipmentSlotEmpty(slot)))
            {
                Logger.LogDebug($"{nameof(DurabilityDisplayService)}::{nameof(TrackEquipmentSlot)}: Tracking Equipment Slot {slot} for Player RewiredID {character.CharacterUI.RewiredID}");
                TrackEquippedItem(character, (Equipment)character.Inventory.Equipment.GetEquippedItem(slot));
            }
        }

        private void SplitScreenManagerPatches_RemoveLocalPlayerAfter(SplitScreenManager splitScreenManager, SplitPlayer player, string playerUID) =>
            ShowHide(player.RewiredID, false);


        private void AddUnequippedTrackers(int playerId)
        {
            Logger.LogDebug($"{nameof(DurabilityDisplayService)}::{nameof(AddUnequippedTrackers)}: Adding Unequipped Helm, Chest and Boots trackers for  for playerId {playerId}");
            var display = GetDurabilityDisplay(playerId);
            display.TrackDurability(UnequippedHelm);
            display.TrackDurability(UnequippedChest);
            display.TrackDurability(UnequippedBoots);

            _unequipedAdded = true;
        }

        private void TrackEquippedItem(Character character, Equipment equipment)
        {
            if (!_equipTracked)
                return;
            var display = GetDurabilityDisplay(character.OwnerPlayerSys.PlayerID);
            if (display == null)
                return;

            var durableSlot = equipment.CurrentEquipmentSlot.SlotType.ToDurableEquipmentSlot();

            if (durableSlot == EquipSlots.None)
                return;

            if (!_unequipedAdded)
                AddUnequippedTrackers(character.OwnerPlayerSys.PlayerID);



            if (equipment.IsIndestructible || !character.Inventory.Equipment.HasItemEquipped(equipment.CurrentEquipmentSlot.SlotType))
            {
                if (Unequipped.TryGetValue(durableSlot, out var unequippedTracker))
                    display.TrackDurability(unequippedTracker);
                else
                    display.StopTracking(durableSlot);

                return;
            }

            Logger.LogDebug($"Tracking durability of equipment {equipment.name} for player {character.OwnerPlayerSys.PlayerID}.");

            display.TrackDurability(new DurabilityTracker(equipment));
        }

        private void UntrackEquippedItem(Character character, Equipment equipment)
        {
            if (!_equipTracked)
                return;

            var display = GetDurabilityDisplay(character.OwnerPlayerSys.PlayerID);
            if (display == null)
                return;

            var durableSlot = equipment.CurrentEquipmentSlot.SlotType.ToDurableEquipmentSlot();

            if (durableSlot == EquipSlots.None)
                return;

            if (!_unequipedAdded)
                AddUnequippedTrackers(character.OwnerPlayerSys.PlayerID);

            if (!character.Inventory.Equipment.IsEquipmentSlotEmpty(equipment.EquipSlot))
                return;

            if (!character.Inventory.Equipment.HasItemEquipped(equipment.CurrentEquipmentSlot.SlotType))
            {
                if (Unequipped.TryGetValue(durableSlot, out var unequippedTracker))
                {
                    display.TrackDurability(unequippedTracker);
                    return;
                }

            }

            display.StopTracking(durableSlot);
            Logger.LogDebug($"Stopped tracking durability of equipment {equipment.name} for player {character.OwnerPlayerSys.PlayerID}.");
        }
        private DurabilityDisplay GetDurabilityDisplay(int playerId)
        {
            if (Psp.Instance.TryGetServicesProvider(playerId, out var usp))
            {
                if (usp.TryGetService<PlayerActionMenus>(out var actionMenus))
                {
                    return actionMenus.DurabilityDisplay;
                }
            }
            return null;
        }
    }
}
