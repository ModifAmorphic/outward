using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Rewired;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.ActionMenus.Extensions;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class SlotDataService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly Player _rewiredPlayer;
        private readonly Character _character;
        private readonly CharacterInventory _inventory;

        public SlotDataService(Player rewiredPlayer, Character character, Func<IModifLogger> getLogger)
        {
            if (rewiredPlayer == null)
                throw new ArgumentNullException(nameof(rewiredPlayer));

            if (character == null)
                throw new ArgumentNullException(nameof(character));

            _rewiredPlayer = rewiredPlayer;
            _character = character;
            _inventory = character.Inventory;
            _getLogger = getLogger;
        }

        public bool TryGetItemSlotAction(SlotData slotData, out ItemSlotAction slotAction)
        {
            if (!TryFindOwnedItem(slotData.ItemID, slotData.ItemUID, out var item))
                if (!TryFindPrefab(slotData.ItemID, out item))
                {
                    slotAction = null;
                    return false;
                }

            slotAction = new ItemSlotAction(item, _rewiredPlayer, _character, this, _getLogger)
            {
                Cooldown = new ItemCooldown(item),
                Stack = item.IsStackable() ? item.ToStackable(_character.Inventory) : null,
            };
            return true;
        }

        public bool TryFindOwnedItem(int itemId, string uid, out Item item)
        {
            item = null;
            var itemSkills = _inventory.SkillKnowledge.GetLearnedItems().Where(s => s.ItemID == itemId);
            if (itemSkills.Any())
            {
                item = itemSkills.FirstOrDefault(s => s.ItemID == itemId) ?? itemSkills.First();
                return true;
            }

            var items = _inventory.GetOwnedItems(itemId);
            if (items != null && items.Any())
            {
                item = items.FirstOrDefault(i => i.UID.Equals(uid));
                if (item == null)
                    item = items.First();
            }            

            return item != null;
        }
        public bool TryFindPrefab(int itemId, out Item item)
        {
            item = ResourcesPrefabManager.Instance.GetItemPrefab(itemId);
            return item != null;
        }
    }
}
