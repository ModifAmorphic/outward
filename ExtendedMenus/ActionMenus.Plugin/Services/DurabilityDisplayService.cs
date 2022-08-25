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

        public DurabilityDisplayService( Func<IModifLogger> loggerFactory)
        {
            (_loggerFactory) = (loggerFactory);

            EquipmentPatches.AfterOnEquip += TrackEquippedItem;
        }

        private void AddUnequipedTrackers(int playerId)
        {
            var psp = Psp.Instance.GetServicesProvider(playerId);
            var display = psp.GetService<DurabilityDisplay>();
            display.TrackDurability(new UnequippedDurabilityTracker(DurableEquipmentSlot.Head, DurableEquipmentType.Helm, 1f));
            display.TrackDurability(new UnequippedDurabilityTracker(DurableEquipmentSlot.Chest, DurableEquipmentType.Chest, 1f));
            display.TrackDurability(new UnequippedDurabilityTracker(DurableEquipmentSlot.Feet, DurableEquipmentType.Boots, 1f));

            _unequipedAdded = true;
        }

        private void TrackEquippedItem(Character character, Equipment equipment)
        {
            if (!_unequipedAdded)
                AddUnequipedTrackers(character.OwnerPlayerSys.PlayerID);

            if (equipment.CurrentEquipmentSlot.SlotType.ToDurableEquipmentSlot() == DurableEquipmentSlot.None
                || equipment.IsIndestructible)
                return;

            Logger.LogDebug($"Tracking durability of equipment {equipment.name} for player {character.OwnerPlayerSys.PlayerID}.");
            var psp = Psp.Instance.GetServicesProvider(character.OwnerPlayerSys.PlayerID);
            var display = psp.GetService<DurabilityDisplay>();
            display.TrackDurability(new DurabilityTracker(equipment));
        }
    }
}
