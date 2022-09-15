using ModifAmorphic.Outward.UI.DataModels;
using ModifAmorphic.Outward.UI.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Rewired;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.Extensions;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Coroutines;

namespace ModifAmorphic.Outward.UI.Services
{
    internal class SlotDataService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly Player _rewiredPlayer;
        private readonly Character _character;
        private readonly CharacterInventory _inventory;
        private readonly HotbarProfileJsonService _profileService;

        public SlotDataService(Player rewiredPlayer, Character character, HotbarProfileJsonService profileService, Func<IModifLogger> getLogger)
        {
            if (rewiredPlayer == null)
                throw new ArgumentNullException(nameof(rewiredPlayer));

            if (character == null)
                throw new ArgumentNullException(nameof(character));

            _rewiredPlayer = rewiredPlayer;
            _character = character;
            _inventory = character.Inventory;
            _profileService = profileService;
            _getLogger = getLogger;
        }

        public bool TryGetItemSlotAction(SlotData slotData, bool combatModeEnabled, out ISlotAction slotAction)
        {
            slotAction = null;
            if (!TryFindOwnedItem(slotData.ItemID, slotData.ItemUID, out var item))
                if (!TryFindPrefab(slotData.ItemID, out item))
                    return false;

            if (item is Skill skill)
            {
                slotAction = new SkillSlotAction(skill, _rewiredPlayer, _character, this, combatModeEnabled, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(skill)
                };
            }
            else if (item is Equipment equipment)
            {
                slotAction = new EquipmentSlotAction(equipment, _rewiredPlayer, _character, this, combatModeEnabled, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(equipment)
                };
            }
            else
            {
                slotAction = new ItemSlotAction(item, _rewiredPlayer, _character, this, combatModeEnabled, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(item),
                    Stack = item.IsStackable() ? item.ToStackable(_character.Inventory) : null,
                };
            }

            return slotAction != null;
        }

        public bool TryFindOwnedItem(int itemId, string uid, out Item item)
        {
            item = null;
            //Search skills
            var itemSkills = _inventory.SkillKnowledge.GetLearnedItems().Where(s => s.ItemID == itemId);
            if (itemSkills.Any())
            {
                item = itemSkills.FirstOrDefault(s => s.ItemID == itemId) ?? itemSkills.First();
                return true;
            }

            //If not owned, stop looking
            if (!_inventory.OwnsOrHasEquipped(itemId))
                return false;

            //Find by uid
            if (!string.IsNullOrEmpty(uid))
            {
                var worldItem = ItemManager.Instance.GetItem(uid);
                if (worldItem != null && worldItem.OwnerCharacter == _character)
                {
                    item = worldItem;
                    return true;
                }
            }

            //UID not found or empty. Check inventory for ItemID match
            var items = _inventory.GetOwnedItems(itemId);
            if (items != null && items.Any())
            {
                item = items.First();
                return true;
            }
            //Check equipment for ItemID
            _inventory.Equipment.OwnsItem(itemId, out item);

            return item != null;
        }
        public bool TryFindPrefab(int itemId, out Item item)
        {
            item = ResourcesPrefabManager.Instance.GetItemPrefab(itemId);
            return item != null;
        }

        public ISlotAction GetSlotAction(Item item)
        {
            if (item is Skill skill)
            {
                return new SkillSlotAction(skill, _rewiredPlayer, _character, this, _profileService.GetProfile()?.CombatMode ?? true, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(item),
                };
            }
            else if (item is Equipment equipment)
            {
                return new EquipmentSlotAction(equipment, _rewiredPlayer, _character, this, _profileService.GetProfile()?.CombatMode ?? true, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(item),
                };
            }
            else
            {
                return new ItemSlotAction(item, _rewiredPlayer, _character, this, _profileService.GetProfile()?.CombatMode ?? true, _getLogger)
                {
                    Cooldown = new ItemCooldownTracker(item),
                    Stack = item.IsStackable() ? item.ToStackable(_character.Inventory) : null
                };
            }
        }
    }
}
