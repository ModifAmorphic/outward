using ModifAmorphic.Outward.ActionMenus.DataModels;
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
        private readonly SlotDataService _slotData;
        private readonly HotbarProfileJsonService _profileService;

        public SlotActionViewData(Player player, Character character, SlotDataService slotData, HotbarProfileJsonService profileService, Func<IModifLogger> getLogger)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (character == null)
                throw new ArgumentNullException(nameof(character));

            _player = player;
            _character = character;
            _slotData = slotData;
            _inventory = character.Inventory;
            _profileService = profileService;
            _getLogger = getLogger;
        }
        public IEnumerable<IActionsDisplayTab> GetActionsTabData()
        {
            var displays = new List<ActionsDisplayTab>();
            int tabOrder = 0;
            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.SkillsTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetSkills()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.ConsumablesTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetConsumables()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.DeployablesTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetDeployables()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionMenuSettings.ActionViewer.EquippedTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetEquipped()
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
                GetSlotActionsQuery = () => GetArmor()
            });

            return displays;
        }

        public IEnumerable<ISlotAction> GetAllActions()
        {
            var allItems = new List<ISlotAction>();

            allItems.AddRange(GetArmor());
            allItems.AddRange(GetWeapons());
            allItems.AddRange(GetEquipped());
            allItems.AddRange(GetDeployables());
            allItems.AddRange(GetConsumables());

            return allItems.OrderBy(i => i.DisplayName);
        }
        public IEnumerable<ISlotAction> GetWeapons()
        {
            var allWeapons = new List<ISlotAction>();

            allWeapons.AddRange(_inventory.Pouch.GetContainedItems().Where(i => i is Weapon).Select(w => GetSlotAction(w)));
            if (_inventory.HasABag)
                allWeapons.AddRange(_inventory.EquippedBag.Container.GetContainedItems().Where(i => i is Weapon).Select(w => GetSlotAction(w)));
            
            return allWeapons;
        }
        public IEnumerable<ISlotAction> GetSkills() => _inventory.SkillKnowledge.GetLearnedItems().Select(s => GetSlotAction(s));
        public IEnumerable<ISlotAction> GetConsumables() => _inventory.GetOwnedItems(TagSourceManager.Consumable)
                                                      .GroupBy(i => i.ItemID)
                                                      .Select(i => GetSlotAction(i.First()));
        public IEnumerable<ISlotAction> GetDeployables() => _inventory.GetOwnedItems(TagSourceManager.Deployable).Select(s => GetSlotAction(s));

        public IEnumerable<ISlotAction> GetEquipped() => _inventory.Equipment.EquipmentSlots.Where(s => s != null && s.HasItemEquipped).Select(e => GetSlotAction(e.EquippedItem));
        public IEnumerable<ISlotAction> GetArmor() => _inventory.GetOwnedItems(TagSourceManager.Armor).Select(s => GetSlotAction(s));


        private ISlotAction GetSlotAction(Item item)
        {
            if (item is Skill skill)
            {
                return new SkillSlotAction(skill, _player, _character, _slotData, _profileService.GetProfile()?.CombatMode ?? true, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(item),
                };
            }
            else if (item is Equipment equipment)
            {
                return new EquipmentSlotAction(equipment, _player, _character, _slotData, _profileService.GetProfile()?.CombatMode ?? true, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(item),
                };
            }
            else
            {
                return new ItemSlotAction(item, _player, _character, _slotData, _profileService.GetProfile()?.CombatMode ?? true, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(item),
                    Stack = item.IsStackable() ? item.ToStackable(_character.Inventory) : null
                };
            }
        }
    }
}
