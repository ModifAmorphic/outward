using ModifAmorphic.Outward.Unity.ActionUI;
using System;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class StackTracker : IStackable
    {
        private IOutwardItem _outwardItem;
        private readonly CharacterInventory _inventory;
        public bool IsStackable => true;

        public StackTracker(IOutwardItem item, CharacterInventory inventory)
        {
            if (item == null || item.ActionItem == null)
                throw new ArgumentNullException(nameof(item));
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            _outwardItem = item;
            _inventory = inventory;
        }
        public int GetAmount()
        {
            return _outwardItem.ActionItem.GroupItemInDisplay || _outwardItem.ActionItem.IsStackable ? _inventory.ItemCount(_outwardItem.ActionItem.ItemID) : _outwardItem.ActionItem.QuickSlotCountDisplay;
        }

    }
}
