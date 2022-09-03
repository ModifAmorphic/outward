using ModifAmorphic.Outward.ActionMenus.Extensions;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class DurabilityDisplayService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private bool _unequipedAdded;

        private readonly static UnequippedDurabilityTracker UnequippedHelm = new UnequippedDurabilityTracker(DurableEquipmentSlot.Head, DurableEquipmentType.Helm, 1f);
        private readonly static UnequippedDurabilityTracker UnequippedChest = new UnequippedDurabilityTracker(DurableEquipmentSlot.Chest, DurableEquipmentType.Chest, 1f);
        private readonly static UnequippedDurabilityTracker UnequippedBoots = new UnequippedDurabilityTracker(DurableEquipmentSlot.Feet, DurableEquipmentType.Boots, 1f);

        private readonly static Dictionary<DurableEquipmentSlot, UnequippedDurabilityTracker> Unequipped = new Dictionary<DurableEquipmentSlot, UnequippedDurabilityTracker>()
        {
            { UnequippedHelm.DurableEquipmentSlot, UnequippedHelm },
            { UnequippedChest.DurableEquipmentSlot, UnequippedChest },
            { UnequippedBoots.DurableEquipmentSlot, UnequippedBoots }
        };

        public DurabilityDisplayService( Func<IModifLogger> loggerFactory)
        {
            (_loggerFactory) = (loggerFactory);

            EquipmentPatches.AfterOnEquip += TrackEquippedItem;
            EquipmentPatches.AfterOnUnequip += UntrackEquippedItem;
        }

        private void AddUnequipedTrackers(int playerId)
        {
            var psp = Psp.Instance.GetServicesProvider(playerId);
            var display = psp.GetService<DurabilityDisplay>();
            display.TrackDurability(UnequippedHelm);
            display.TrackDurability(UnequippedChest);
            display.TrackDurability(UnequippedBoots);

            _unequipedAdded = true;
        }

        private void TrackEquippedItem(Character character, Equipment equipment)
        {
            if (!_unequipedAdded)
                AddUnequipedTrackers(character.OwnerPlayerSys.PlayerID);

            var psp = Psp.Instance.GetServicesProvider(character.OwnerPlayerSys.PlayerID);
            var display = psp.GetService<DurabilityDisplay>();
            var durableSlot = equipment.CurrentEquipmentSlot.SlotType.ToDurableEquipmentSlot();

            if (durableSlot == DurableEquipmentSlot.None
                || equipment.IsIndestructible)
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
            if (!_unequipedAdded)
                AddUnequipedTrackers(character.OwnerPlayerSys.PlayerID);

            if (!character.Inventory.Equipment.IsEquipmentSlotEmpty(equipment.EquipSlot))
                return;

            var psp = Psp.Instance.GetServicesProvider(character.OwnerPlayerSys.PlayerID);
            var display = psp.GetService<DurabilityDisplay>();
            var durableSlot = equipment.CurrentEquipmentSlot.SlotType.ToDurableEquipmentSlot();

            if (durableSlot == DurableEquipmentSlot.None
                || equipment.IsIndestructible)
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
    }
}
