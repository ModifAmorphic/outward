using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Services;
using ModifAmorphic.Outward.Unity.ActionMenus;
using Rewired;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.UI.Models
{
    internal class ItemSlotAction : ISlotAction, IOutwardItem
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

        private int hotbarIndex;
        private int slotIndex;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public bool IsEditable { get; internal set; }

        public string DisplayName { get; internal set; }

        public ICooldown Cooldown { get; internal set; }

        public IStackable Stack { get; internal set; }

        public Dictionary<BarPositions, IBarProgress> ActiveBars => null;

        public ActionSlotIcon[] ActionIcons { get; internal set; }

        public bool HasDynamicIcon { get; internal set; }

        public bool IsCombatModeEnabled { get; private set; }

        public event Action OnActionRequested;
        public event Action OnEditRequested;
        public event Action<ActionSlotIcon[]> OnIconsChanged;

        public Action TargetAction => TryActivateTarget;

        public bool CheckOnUpdate { get; internal set; } = true;


        public ItemSlotAction(Item item, Player player, Character character, SlotDataService slotData, bool combatModeEnabled, Func<IModifLogger> getLogger)
        {
            (this.item, this.player, this.character, this.slotData, _getLogger) = (item, player, character, slotData, getLogger);

            this.inventory = character.Inventory;
            this.ActionIcons = GetItemSprites(false);
            this.DisplayName = item.DisplayName;
            this.HasDynamicIcon = item.HasDynamicQuickSlotIcon;
            this.IsCombatModeEnabled = combatModeEnabled;

            itemId = item.ItemID;
            itemUid = item.UID;
        }


        public ActionSlotIcon[] GetDynamicIcons()
        {
            bool isLocked;
            if (item is Skill skill)
                isLocked = !skill.IsChildToCharacter || !skill.GetIsQuickSlotReq();
            else
                isLocked = item is Equipment equipment && equipment.IsEquipped || !inventory.OwnsItem(item.ItemID);

            return GetItemSprites(isLocked);
        }
        private static HashSet<string> ignored = new HashSet<string>()
        {
            "Icon", "border", "break", "background", "Dot", "Background", "Fill", "imgNew", "imgHighlight", "Backbround", "CoinIcon"
        };
        private ActionSlotIcon[] GetItemSprites(bool isLocked)
        {
            var actionIcon = isLocked ? item.LockedIcon ?? item.QuickSlotIcon ?? item.ItemIcon : item.QuickSlotIcon;

            var itemDisplay = item.GetPrivateField<Item, ItemDisplay>("m_refItemDisplay");
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

            return sprites.ToArray();
        }

        public bool GetIsActionRequested()
        {
            return !IsEditable && player.GetButtonDown(actionConfig.RewiredActionId) && GetEnabled();
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
            if (item == null)
                return;

            if (inventory.OwnsItem(item.UID) || (item is Skill skill && inventory.LearnedSkill(skill)))
            {
                item.TryQuickSlotUse();
            }
            else
            {
                character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(item.Name, Global.HIGHLIGHT)));
            }
        }

        public bool GetEnabled()
        {
            if (IsCombatModeEnabled && character.InCombat && (hotbarIndex != 0 || slotIndex > 8))
                return false;

            if (item is Skill skill && skill.IsChildToCharacter && skill.GetIsQuickSlotReq())
                return true;

            else if (item is Equipment equip && inventory.OwnsItem(item.UID))
            {
                if (!equip.IsEquipped)
                    return true;
                else
                    return false;
            }

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
            hotbarIndex = assignedSlot.HotbarIndex;
            slotIndex = assignedSlot.SlotIndex;
            item.SetQuickSlot(slotIndex + 1);
            actionConfig = (ActionConfig)assignedSlot.Config;
        }
        public void SlotActionUnassigned()
        {
            item.SetQuickSlot(-1);
        }
    }
}
