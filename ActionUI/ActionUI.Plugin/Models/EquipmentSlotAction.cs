using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using Rewired;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class EquipmentSlotAction : SlotActionBase
    {
        private Equipment equipment;
        public Equipment ActionEquipment => equipment;

        private bool isEquipBroken;

        public EquipmentSlotAction(Equipment equipment, Player player, Character character, SlotDataService slotData, bool combatModeEnabled, Func<IModifLogger> getLogger)
            : base(equipment, player, character, slotData, combatModeEnabled, getLogger)
        {
            this.equipment = equipment;

            if (!equipment.IsIndestructible)
                this._activeBars.Add(BarPositions.Right, new DurabilityActionSlotTracker(this, BarPositions.Right));
        }

        public override bool GetEnabled()
        {
            if (IsCombatModeEnabled && character.InCombat && (hotbarIndex != 0 || slotIndex > 8))
                return false;

            if (inventory.OwnsItem(itemUid))
            {
                if (!actionItem.IsEquipped)
                    return true;
                else
                    return false;
            }

            if (slotData.TryFindOwnedItem(ActionId, ActionUid, out var foundItem) && foundItem is Equipment equip)
            {
                actionItem = equip;
                itemUid = foundItem.UID;
                actionItem.SetQuickSlot(0);
                return true;
            }

            return false;
        }

        public override void ActivateTarget()
        {
            if (actionItem == null)
                return;

            if (inventory.OwnsItem(actionItem.UID))
            {
                actionItem.TryQuickSlotUse();
            }
            else
            {
                character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(actionItem.Name, Global.HIGHLIGHT)));
            }
        }

        protected override List<ActionSlotIcon> GetActionIcons(bool isLocked)
        {
            var sprites = base.GetActionIcons(isLocked);

            if (isEquipBroken)
            {
                Logger.LogDebug($"SlotIndex: {slotIndex}. Adding CrackedIcon to Sprites for slot.");
                sprites.Add(new ActionSlotIcon() { Name = "CrackedIcon", Icon = ActionMenuResources.Instance.SpriteResources["CrackedIcon"], IsTopSprite = true });
            }
            return sprites;
        }

        internal void DurabilityChanged(float durability)
        {
            Logger.LogDebug($"SlotIndex: {slotIndex}. Durability changed to {durability}. isEquipBroken is {isEquipBroken}.");
            if (durability > 0f && isEquipBroken)
            {
                isEquipBroken = false;
                RaiseOnIconsChanged();
            }
            else if (Mathf.Approximately(durability, 0f) && !isEquipBroken)
            {
                isEquipBroken = true;
                RaiseOnIconsChanged();
            }

            Logger.LogDebug($"SlotIndex: {slotIndex}. Durability changed done. isEquipBroken is {isEquipBroken}.");
        }

        protected override bool GetIsLocked() => equipment.IsEquipped || !inventory.OwnsItem(equipment.ItemID);
    }
}
