using ModifAmorphic.Outward.Unity.ActionUI;
using System;

namespace ModifAmorphic.Outward.UI.Models
{
    internal class StackTracker : IStackable
    {
        private readonly Item _item;
        private readonly CharacterInventory _inventory;
        public bool IsStackable => true;

        public StackTracker(Item item, CharacterInventory inventory)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            _item = item;
            _inventory = inventory;
        }
        public int GetAmount()
        {
            return _item.GroupItemInDisplay || _item.IsStackable ? _inventory.ItemCount(_item.ItemID) : _item.QuickSlotCountDisplay;
        }
    }
}
