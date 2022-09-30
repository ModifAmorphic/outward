using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.ActionUI.Services
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
                DisplayName = ActionUISettings.ActionViewer.SkillsTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetSkills()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionUISettings.ActionViewer.ConsumablesTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetConsumables()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionUISettings.ActionViewer.DeployablesTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetDeployables()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionUISettings.ActionViewer.EquippedTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetEquipped()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionUISettings.ActionViewer.WeaponsTab,
                TabOrder = tabOrder++,
                GetSlotActionsQuery = () => GetWeapons()
            });

            displays.Add(new ActionsDisplayTab()
            {
                DisplayName = ActionUISettings.ActionViewer.ArmorTab,
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

            allWeapons.AddRange(_inventory.Pouch.GetContainedItems().Where(i => i is Weapon).Select(w => _slotData.GetSlotAction(w)));
            if (_inventory.HasABag)
                allWeapons.AddRange(_inventory.EquippedBag.Container.GetContainedItems().Where(i => i is Weapon).Select(w => _slotData.GetSlotAction(w)));

            return allWeapons;
        }
        public IEnumerable<ISlotAction> GetSkills() => _inventory.SkillKnowledge.GetLearnedItems().Select(s => _slotData.GetSlotAction(s));
        public IEnumerable<ISlotAction> GetConsumables() => _inventory.GetOwnedItems(TagSourceManager.Consumable)
                                                      .GroupBy(i => i.ItemID)
                                                      .Select(i => _slotData.GetSlotAction(i.First()));
        public IEnumerable<ISlotAction> GetDeployables() => _inventory.GetOwnedItems(TagSourceManager.Deployable).Select(s => _slotData.GetSlotAction(s));

        public IEnumerable<ISlotAction> GetEquipped() => _inventory.Equipment.EquipmentSlots.Where(s => s != null && s.HasItemEquipped).Select(e => _slotData.GetSlotAction(e.EquippedItem));
        public IEnumerable<ISlotAction> GetArmor() => _inventory.GetOwnedItems(TagSourceManager.Armor).Select(s => _slotData.GetSlotAction(s));
    }
}
