using ModifAmorphic.Outward.ActionUI.Extensions;
using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using Rewired;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal abstract class SlotActionBase : ISlotAction, IOutwardItem
    {
        protected readonly Player player;
        protected readonly Character character;
        protected readonly CharacterInventory inventory;
        protected readonly SlotDataService slotData;

        protected readonly int itemId;

        protected readonly Dictionary<BarPositions, IBarProgress> _activeBars = new Dictionary<BarPositions, IBarProgress>();

        protected string itemUid;

        protected int hotbarIndex;
        protected int slotIndex;

        protected ItemDisplay refItemDisplay;

        protected ActionConfig actionConfig;

        protected Item actionItem;
        public Item ActionItem => actionItem;
        public int ActionId => itemId;
        public string ActionUid => itemUid;

        public bool IsEditable { get; internal set; }

        public string DisplayName { get; internal set; }

        public ICooldown Cooldown { get; internal set; }

        public IStackable Stack { get; internal set; }
        public Dictionary<BarPositions, IBarProgress> ActiveBars => _activeBars;

        public ActionSlotIcon[] ActionIcons { get; internal set; }

        public bool HasDynamicIcon { get; internal set; }

        public bool IsCombatModeEnabled { get; private set; }

        public Action TargetAction => TryActivateTarget;

        public bool CheckOnUpdate { get; internal set; } = true;

        private readonly Func<IModifLogger> _getLogger;
        protected IModifLogger Logger => _getLogger.Invoke();

        public event Action OnActionRequested;
        public event Action OnEditRequested;
        public event Action<ActionSlotIcon[]> OnIconsChanged;


        public SlotActionBase(Item item, Player player, Character character, SlotDataService slotData, bool combatModeEnabled, Func<IModifLogger> getLogger)
        {
            (this.actionItem, this.player, this.character, this.slotData, _getLogger) = (item, player, character, slotData, getLogger);

            this.inventory = character.Inventory;
            this.ActionIcons = GetActionIcons(false).ToArray();
            this.DisplayName = item.DisplayName;
            this.HasDynamicIcon = item.HasDynamicQuickSlotIcon;
            this.IsCombatModeEnabled = combatModeEnabled;
            this.Stack = item.IsStackable() ? new StackTracker(this, character.Inventory) : null;

            itemId = item.ItemID;
            itemUid = item.UID;
        }

        public abstract void ActivateTarget();

        public ActionSlotIcon[] GetDynamicIcons() => GetActionIcons(GetIsLocked()).ToArray();

        public abstract bool GetEnabled();

        public bool GetIsActionRequested() => !IsEditable && player.GetButtonDown(actionConfig.RewiredActionId);

        public bool GetIsEditRequested() => IsEditable && player.GetButtonDown(actionConfig.RewiredActionId);

        public void SlotActionAssigned(ActionSlot assignedSlot)
        {
            hotbarIndex = assignedSlot.HotbarIndex;
            slotIndex = assignedSlot.SlotIndex;
            actionItem.SetQuickSlot(0);
            actionConfig = (ActionConfig)assignedSlot.Config;

            if (refItemDisplay != null)
                refItemDisplay.gameObject.Destroy();

            AssignRefItemDisplay(assignedSlot);
            this.OnIconsChanged?.Invoke(GetActionIcons(false).ToArray());
        }

        protected virtual void AssignRefItemDisplay(ActionSlot assignedSlot)
        {
            if (UIUtilities.ItemDisplayPrefab != null)
            {
                refItemDisplay = UnityEngine.Object.Instantiate<ItemDisplay>(UIUtilities.ItemDisplayPrefab);
                refItemDisplay.transform.SetParent(assignedSlot.ActionImages.transform);
                refItemDisplay.transform.SetAsLastSibling();
                refItemDisplay.transform.ResetLocal();
                refItemDisplay.SetReferencedItem(this.actionItem);
                refItemDisplay.gameObject.SetActive(false);
            }
        }

        protected void RaiseOnActionRequested() => OnActionRequested?.Invoke();
        protected void RaiseOnEditRequested() => OnEditRequested?.Invoke();
        protected void RaiseOnIconsChanged() => OnIconsChanged?.Invoke(GetActionIcons(GetIsLocked()).ToArray());

        protected abstract bool GetIsLocked();

        public virtual void SlotActionUnassigned()
        {
            actionItem.SetQuickSlot(-1);
        }

        private static HashSet<string> ignoredIcons = new HashSet<string>()
        {
            "Icon", "border", "break", "background", "Dot", "Background", "Fill", "imgNew", "imgHighlight", "Backbround", "CoinIcon"
        };

        protected virtual List<ActionSlotIcon> GetActionIcons(bool isLocked)
        {
            var actionIcon = isLocked ? actionItem.LockedIcon ?? actionItem.QuickSlotIcon ?? actionItem.ItemIcon : actionItem.QuickSlotIcon;

            var itemDisplay = actionItem.GetPrivateField<Item, ItemDisplay>("m_refItemDisplay");
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

                    if (image != null && image.gameObject.activeSelf && !ignoredIcons.Contains(image.name) && (image.sprite != null || image.overrideSprite != null))
                        sprites.Add(new ActionSlotIcon() { Name = image.name, Icon = image.overrideSprite ?? image.sprite });
                }
            }
#if DEBUG
            //Logger.LogTrace($"SlotIndex: {slotIndex}. Got {sprites.Count} Sprites.");
#endif

            return sprites;
        }

        private void TryActivateTarget()
        {
            try
            {
                ActivateTarget();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to activate {actionConfig.RewiredActionName} item/skill {actionItem?.name} for character {character?.name}", ex);
            }
        }
    }
}