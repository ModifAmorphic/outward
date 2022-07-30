using ModifAmorphic.Outward.ActionMenus.Extensions;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class SlotActionViewData : IActionViewData
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly Player _player;
        private readonly Character _character;
        private readonly CharacterInventory _inventory;

        public SlotActionViewData(Player player, Character character, Func<IModifLogger> getLogger)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (character == null)
                throw new ArgumentNullException(nameof(character));

            _player = player;
            _character = character;
            _inventory = character.Inventory;
        }
        public IEnumerable<IActionsDisplayTab> GetActionsTabData()
        {
            var displays = new List<ActionsDisplayTab>();
            int tabOrder = 0;
            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.SkillsTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => _inventory.SkillKnowledge.GetLearnedItems().Select(s => GetSlotAction(s))
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.ConsumablesTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => _inventory.GetOwnedItems(TagSourceManager.Consumable)
                                                      .GroupBy(i => i.ItemID)
                                                      .Select(i => GetSlotAction(i.First()))
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.DeployablesTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => _inventory.GetOwnedItems(TagSourceManager.Deployable).Select(s => GetSlotAction(s))
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.EquippedTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => _inventory.Equipment.EquipmentSlots.Where(s => s != null && s.HasItemEquipped).Select(e => GetSlotAction(e.EquippedItem))
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.WeaponsTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetWeapons()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.ArmorTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => _inventory.GetOwnedItems(TagSourceManager.Armor).Select(s => GetSlotAction(s))
            });

            return displays;
        }

        public IEnumerable<ISlotAction> GetAllActions()
        {
            var allItems = new List<Item>();

            //allItems.AddRange(_inventory.SkillKnowledge.GetLearnedItems());

            allItems.AddRange(_inventory.Equipment.EquipmentSlots.Where(s => s != null && s.HasItemEquipped).Select(e => e.EquippedItem));

            allItems.AddRange(_inventory.Pouch.GetContainedItems());
            if (_inventory.HasABag)
                allItems.AddRange(_inventory.EquippedBag.Container.GetContainedItems());

            return allItems.Where(i => i.IsQuickSlotable).Select(i => GetSlotAction(i));
        }
        public IEnumerable<ISlotAction> GetWeapons()
        {
            var allWeapons = new List<ISlotAction>();

            allWeapons.AddRange(_inventory.Pouch.GetContainedItems().Where(i => i is Weapon).Select(w => GetSlotAction(w)));
            if (_inventory.HasABag)
                allWeapons.AddRange(_inventory.EquippedBag.Container.GetContainedItems().Where(i => i is Weapon).Select(w => GetSlotAction(w)));
            
            return allWeapons;
        }
        private ISlotAction GetSlotAction(Item item)
        {
            var slotAction = new ItemSlotAction(item, _player, _character, _getLogger)
            {
                DisplayName = item.DisplayName,
                ActionIcon = item.QuickSlotIcon,
                Cooldown = new ItemCooldown(item),
                Stack = item.IsStackable() ? item.ToStackable(_character.Inventory) : null,
                HasDynamicIcon = item.HasDynamicQuickSlotIcon
            };
            return slotAction;
        }
    }
}
