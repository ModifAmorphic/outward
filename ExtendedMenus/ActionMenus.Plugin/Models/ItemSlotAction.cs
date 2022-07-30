using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionMenus.Models
{
    internal class ItemSlotAction : ISlotAction
    {
        private string rewiredActionName;
        private readonly Player player;
        private readonly Character character;
        private readonly CharacterInventory inventory;
        private readonly Item target;
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

        public ItemSlotAction(Item item, Player player, Character character, Func<IModifLogger> getLogger)
        {
            (this.target, this.player, this.character, _getLogger) = (item, player, character, getLogger);
            
            this.inventory = character.Inventory;
            this.ActionIcon = item.QuickSlotIcon;
        }


        public Sprite GetDynamicIcon()
        {
            bool isLocked;
            if (target is Skill skill)
                isLocked = !skill.IsChildToCharacter || !skill.GetIsQuickSlotReq();
            else
                isLocked = target is Equipment equipment && equipment.IsEquipped || !inventory.OwnsItem(target.ItemID);

            return isLocked ? target.LockedIcon?? target.QuickSlotIcon?? target.ItemIcon : target.QuickSlotIcon;
        }

        public bool GetIsActionRequested()
        {
            return !IsEditable && player.GetButtonDown(rewiredActionName);
        }
        public bool GetIsEditRequested()
        {
            return IsEditable && player.GetButtonDown(rewiredActionName);
        }

        private void TryActivateTarget()
        {
            try
            {
                ActivateTarget();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to activate {rewiredActionName} item/skill {target?.name} for character {character?.name}", ex);
            }
        }
        public void ActivateTarget()
        {
            //this.CheckAndUpdateRefItem();
            if (target == null)
                return;

            if (inventory.OwnsItem(target.UID) || (target is Skill skill && inventory.LearnedSkill(skill)))
            {
                //if ((Object)this.m_activeItem != (Object)this.m_registeredItem)
                //    this.SetQuickSlot(this.m_activeItem);
                target.TryQuickSlotUse();
            }
            else
            {
                //if (!(bool)(Object)this.m_owner || !this.m_owner.IsLocalPlayer || !(bool)(Object)this.m_owner.CharacterUI)
                //    return;
                character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(target.Name, Global.HIGHLIGHT)));
            }
        }

        public bool GetEnabled()
        {
            if (target is Skill skill)
            {
                return target != null && skill.IsChildToCharacter && skill.GetIsQuickSlotReq();
            }
            else
            {
                return target != null && !(target is Equipment equip && equip.IsEquipped) && inventory.OwnsItem(target.ItemID);
            }
        }

        public void SlotActionAssigned(ActionSlot assignedSlot)
        {
            int slotNo = assignedSlot.SlotIndex + 1;
            rewiredActionName = slotNo.ToString(RewiredConstants.ActionSlots.NameFormat);
            target.SetQuickSlot(slotNo);
            //assignedSlot.MouseClickListener.OnRightClick.AddListener(() => OnEditRequested?.Invoke());
        }
        public void SlotActionUnassigned()
        {
            target.SetQuickSlot(-1);
        }
    }
}
