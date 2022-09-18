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
    internal class SkillSlotAction : ISlotAction, IOutwardItem
    {
        private readonly Player player;
        private readonly Character character;
        private readonly CharacterInventory inventory;
        private readonly SlotDataService slotData;

        private ActionConfig actionConfig;

        private Skill skill;
        public Skill ActionSkill => skill;
        public Item ActionItem => skill;

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

        public IStackable Stack => null;

        public Dictionary<BarPositions, IBarProgress> ActiveBars => null;

        public IBarProgress RightSlider => null;

        public ActionSlotIcon[] ActionIcons { get; internal set; }

        public bool HasDynamicIcon { get; internal set; }

        public bool IsCombatModeEnabled { get; private set; }

        public event Action OnActionRequested;
        public event Action OnEditRequested;
        public event Action<ActionSlotIcon[]> OnIconsChanged;

        public Action TargetAction => TryActivateTarget;

        public bool CheckOnUpdate { get; internal set; } = true;

        public SkillSlotAction(Skill skill, Player player, Character character, SlotDataService slotData, bool combatModeEnabled, Func<IModifLogger> getLogger)
        {
            (this.skill, this.player, this.character, this.slotData, _getLogger) = (skill, player, character, slotData, getLogger);

            this.inventory = character.Inventory;
            this.ActionIcons = GetSkillSprites(false);
            this.DisplayName = skill.DisplayName;
            this.HasDynamicIcon = skill.HasDynamicQuickSlotIcon;
            this.IsCombatModeEnabled = combatModeEnabled;

            itemId = skill.ItemID;
            itemUid = skill.UID;
        }


        public ActionSlotIcon[] GetDynamicIcons()
        {
            bool isLocked = !skill.IsChildToCharacter || !skill.GetIsQuickSlotReq();

            return GetSkillSprites(isLocked);
        }
        private static HashSet<string> ignored = new HashSet<string>()
        {
            "Icon", "border", "break", "background", "Dot", "Background", "Fill", "imgNew", "imgHighlight", "Backbround", "CoinIcon"
        };
        private ActionSlotIcon[] GetSkillSprites(bool isLocked)
        {
            var actionIcon = isLocked ? skill.LockedIcon ?? skill.QuickSlotIcon ?? skill.ItemIcon : skill.QuickSlotIcon;

            var itemDisplay = skill.GetPrivateField<Item, ItemDisplay>("m_refItemDisplay");
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
                Logger.LogException($"Failed to activate {actionConfig.RewiredActionName} item/skill {skill?.name} for character {character?.name}", ex);
            }
        }
        public void ActivateTarget()
        {
            if (this.skill == null)
                return;

            if (inventory.LearnedSkill(skill))
            {
                skill.TryQuickSlotUse();
            }
            else
            {
                character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(skill.Name, Global.HIGHLIGHT)));
            }
        }

        public bool GetEnabled()
        {
            if (IsCombatModeEnabled && character.InCombat && (hotbarIndex != 0 || slotIndex > 7))
                return false;

            if (skill.IsChildToCharacter && skill.GetIsQuickSlotReq())
                return true;

            return false;
        }

        public void SlotActionAssigned(ActionSlot assignedSlot)
        {
            hotbarIndex = assignedSlot.HotbarIndex;
            slotIndex = assignedSlot.SlotIndex;
            skill.SetQuickSlot(slotIndex + 1);
            actionConfig = (ActionConfig)assignedSlot.Config;
        }
        public void SlotActionUnassigned()
        {
            skill.SetQuickSlot(-1);
        }
    }
}
