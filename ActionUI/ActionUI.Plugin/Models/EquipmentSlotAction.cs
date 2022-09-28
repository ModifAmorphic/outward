using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using Rewired;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class EquipmentSlotAction : ISlotAction, IOutwardItem
    {
        private readonly Player player;
        private readonly Character character;
        private readonly CharacterInventory inventory;
        private readonly SlotDataService slotData;

        private ActionConfig actionConfig;

        private Equipment equipment;
        public Equipment ActionEquipment => equipment;
        public Item ActionItem => equipment;

        private readonly int itemId;
        public int ActionId => itemId;

        private string itemUid;
        public string ActionUid => itemUid;

        private int hotbarIndex;
        private int slotIndex;
        private bool isLocked => equipment.IsEquipped || !inventory.OwnsItem(equipment.ItemID);
        private bool isEquipBroken;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public bool IsEditable { get; internal set; }

        public string DisplayName { get; internal set; }

        public ICooldown Cooldown { get; internal set; }

        public IStackable Stack { get; internal set; }

        private readonly Dictionary<BarPositions, IBarProgress> _activeBars = new Dictionary<BarPositions, IBarProgress>();
        public Dictionary<BarPositions, IBarProgress> ActiveBars => _activeBars;

        public ActionSlotIcon[] ActionIcons { get; internal set; }

        public bool HasDynamicIcon { get; internal set; }

        public bool IsCombatModeEnabled { get; private set; }

        public event Action OnActionRequested;
        public event Action OnEditRequested;
        public event Action<ActionSlotIcon[]> OnIconsChanged;

        public Action TargetAction => TryActivateTarget;

        public bool CheckOnUpdate { get; internal set; } = true;


        public EquipmentSlotAction(Equipment equipment, Player player, Character character, SlotDataService slotData, bool combatModeEnabled, Func<IModifLogger> getLogger)
        {
            (this.equipment, this.player, this.character, this.slotData, _getLogger) = (equipment, player, character, slotData, getLogger);

            this.inventory = character.Inventory;
            this.ActionIcons = GetItemSprites(false);
            this.DisplayName = equipment.DisplayName;
            this.HasDynamicIcon = equipment.HasDynamicQuickSlotIcon;
            this.IsCombatModeEnabled = combatModeEnabled;
            if (!equipment.IsIndestructible)
                this._activeBars.Add(BarPositions.Right, new DurabilityActionSlotTracker(this, BarPositions.Right));
            //this._activeBars.Add(BarPositions.Right, new DurabilityTracker(equipment, BarPositions.Right));

            itemId = equipment.ItemID;
            itemUid = equipment.UID;
        }


        public ActionSlotIcon[] GetDynamicIcons() => GetItemSprites(isLocked);

        private static HashSet<string> ignored = new HashSet<string>()
        {
            "Icon", "border", "break", "background", "Dot", "Background", "Fill", "imgNew", "imgHighlight", "Backbround", "CoinIcon"
        };
        private ActionSlotIcon[] GetItemSprites(bool isLocked)
        {
            var actionIcon = isLocked ? equipment.LockedIcon ?? equipment.QuickSlotIcon ?? equipment.ItemIcon : equipment.QuickSlotIcon;

            var itemDisplay = equipment.GetPrivateField<Item, ItemDisplay>("m_refItemDisplay");
            var sprites = new List<ActionSlotIcon>()
            {
                new ActionSlotIcon() { Name = actionIcon.name, Icon = actionIcon }
            };

            if (itemDisplay != null)
            {
                for (int i = 0; i < itemDisplay.transform.childCount; i++)
                {
                    var child = itemDisplay.transform.GetChild(i);
                    var image = child.GetComponent<Image>();

                    if (image != null && image.gameObject.activeSelf && !ignored.Contains(image.name) && (image.sprite != null || image.overrideSprite != null))
                        sprites.Add(new ActionSlotIcon() { Name = image.name, Icon = image.overrideSprite ?? image.sprite });
                }
            }
            Logger.LogDebug($"SlotIndex: {slotIndex}. Got {sprites.Count} Sprites.");
            if (isEquipBroken)
            {
                Logger.LogDebug($"SlotIndex: {slotIndex}. Adding CrackedIcon to Sprites for slot.");
                sprites.Add(new ActionSlotIcon() { Name = "CrackedIcon", Icon = ActionMenuResources.Instance.SpriteResources["CrackedIcon"], IsTopSprite = true });
            }
            return sprites.ToArray();
        }

        internal void DurabilityChanged(float durability)
        {
            Logger.LogDebug($"SlotIndex: {slotIndex}. Durability changed to {durability}. isEquipBroken is {isEquipBroken}.");
            if (durability > 0f && isEquipBroken)
            {
                isEquipBroken = false;
                OnIconsChanged?.Invoke(GetItemSprites(isLocked));
            }
            else if (Mathf.Approximately(durability, 0f) && !isEquipBroken)
            {
                isEquipBroken = true;
                OnIconsChanged?.Invoke(GetItemSprites(isLocked));
            }

            Logger.LogDebug($"SlotIndex: {slotIndex}. Durability changed done. isEquipBroken is {isEquipBroken}.");
        }

        public bool GetIsActionRequested() => !IsEditable && player.GetButtonDown(actionConfig.RewiredActionId);

        public bool GetIsEditRequested() => IsEditable && player.GetButtonDown(actionConfig.RewiredActionId);

        private void TryActivateTarget()
        {
            try
            {
                ActivateTarget();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to activate {actionConfig.RewiredActionName} item/skill {equipment?.name} for character {character?.name}", ex);
            }
        }

        public void ActivateTarget()
        {
            if (equipment == null)
                return;

            if (inventory.OwnsItem(equipment.UID))
            {
                equipment.TryQuickSlotUse();
            }
            else
            {
                character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(equipment.Name, Global.HIGHLIGHT)));
            }
        }

        public bool GetEnabled()
        {
            if (IsCombatModeEnabled && character.InCombat && (hotbarIndex != 0 || slotIndex > 8))
                return false;

            if (inventory.OwnsItem(equipment.UID))
            {
                if (!equipment.IsEquipped)
                    return true;
                else
                    return false;
            }

            if (slotData.TryFindOwnedItem(ActionId, ActionUid, out var foundItem) && foundItem is Equipment equip)
            {
                equipment = equip;
                itemUid = foundItem.UID;
                equipment.SetQuickSlot(slotIndex + 1);
                return true;
            }

            return false;
        }

        public void SlotActionAssigned(ActionSlot assignedSlot)
        {
            hotbarIndex = assignedSlot.HotbarIndex;
            slotIndex = assignedSlot.SlotIndex;
            equipment.SetQuickSlot(slotIndex + 1);
            actionConfig = (ActionConfig)assignedSlot.Config;
        }
        public void SlotActionUnassigned()
        {
            equipment.SetQuickSlot(-1);
        }
    }
}
