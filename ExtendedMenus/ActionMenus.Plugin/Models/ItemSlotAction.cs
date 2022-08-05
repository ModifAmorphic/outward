using ModifAmorphic.Outward.ActionMenus.Services;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionMenus.Models
{
    internal class ItemSlotAction : ISlotAction
    {
        private readonly Player player;
        private readonly Character character;
        private readonly CharacterInventory inventory;
        private readonly SlotDataService slotData;

        private ActionConfig actionConfig;

        private Item item;
        public Item ActionItem => item;

        private readonly int itemId;
        public int ActionId => itemId;

        private string itemUid;
        public string ActionUid => itemUid;

        private int slotIndex;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public bool IsEditable { get; internal set; }

        public string DisplayName { get; internal set; }

        public ICooldown Cooldown { get; internal set; }

        public IStackable Stack { get; internal set; }

        public Sprite ActionIcon { get; internal set; }

        public bool HasDynamicIcon { get; internal set; }

        public event Action OnActionRequested;
        public event Action OnEditRequested;

        public Action TargetAction => TryActivateTarget;

        public bool CheckOnUpdate { get; internal set; } = true;

        public ItemSlotAction(Item item, Player player, Character character, SlotDataService slotData, Func<IModifLogger> getLogger)
        {
            (this.item, this.player, this.character, this.slotData, _getLogger) = (item, player, character, slotData, getLogger);
            
            this.inventory = character.Inventory;
            this.ActionIcon = item.QuickSlotIcon;
            this.DisplayName = item.DisplayName;
            this.HasDynamicIcon = item.HasDynamicQuickSlotIcon;

            itemId = item.ItemID;
            itemUid = item.UID;
        }


        public Sprite GetDynamicIcon()
        {
            bool isLocked;
            if (item is Skill skill)
                isLocked = !skill.IsChildToCharacter || !skill.GetIsQuickSlotReq();
            else
                isLocked = item is Equipment equipment && equipment.IsEquipped || !inventory.OwnsItem(item.ItemID);

            return isLocked ? item.LockedIcon?? item.QuickSlotIcon?? item.ItemIcon : item.QuickSlotIcon;
        }

        public bool GetIsActionRequested()
        {
            return !IsEditable && player.GetButtonDown(actionConfig.RewiredActionId);
        }
        public bool GetIsEditRequested()
        {
            return IsEditable && player.GetButtonDown(actionConfig.RewiredActionId);
        }

        private void TryActivateTarget()
        {
            try
            {
                ActivateTarget();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to activate {actionConfig.RewiredActionName} item/skill {item?.name} for character {character?.name}", ex);
            }
        }
        public void ActivateTarget()
        {
            //this.CheckAndUpdateRefItem();
            if (item == null)
                return;

            if (inventory.OwnsItem(item.UID) || (item is Skill skill && inventory.LearnedSkill(skill)))
            {
                //if ((Object)this.m_activeItem != (Object)this.m_registeredItem)
                //    this.SetQuickSlot(this.m_activeItem);
                item.TryQuickSlotUse();
            }
            else
            {
                //if (!(bool)(Object)this.m_owner || !this.m_owner.IsLocalPlayer || !(bool)(Object)this.m_owner.CharacterUI)
                //    return;
                character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(item.Name, Global.HIGHLIGHT)));
            }
        }

        public bool GetEnabled()
        {
            if (item is Skill skill && skill.IsChildToCharacter && skill.GetIsQuickSlotReq())
                return true;
            else if (item != null && !(item is Equipment equip && equip.IsEquipped) && inventory.OwnsItem(item.ItemID))
                return true;

            if (slotData.TryFindOwnedItem(ActionId, ActionUid, out var foundItem))
            {
                item = foundItem;
                itemUid = foundItem.UID;
                item.SetQuickSlot(slotIndex + 1);
                return true;
            }

            return false;
        }

        public void SlotActionAssigned(ActionSlot assignedSlot)
        {
            slotIndex = assignedSlot.SlotIndex + 1;
            item.SetQuickSlot(slotIndex + 1);
            actionConfig = (ActionConfig)assignedSlot.Config;

            //SlotAssignment = new SlotAssignment()
            //{
            //    SlotIndex = assignedSlot.SlotIndex,
            //    HotbarIndex = assignedSlot.HotbarIndex,
            //    ItemId = target.ItemID,
            //    ItemUID = target.UID
            //};
            //assignedSlot.MouseClickListener.OnRightClick.AddListener(() => OnEditRequested?.Invoke());
        }
        public void SlotActionUnassigned()
        {
            item.SetQuickSlot(-1);
        }
    }
}
