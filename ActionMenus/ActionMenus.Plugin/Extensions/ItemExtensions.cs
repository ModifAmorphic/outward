using ModifAmorphic.Outward.ActionMenus.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Extensions
{
    internal static class ItemExtensions
    {
        public static bool IsStackable(this Item item) => item.IsStackable || item.HasMultipleUses || item.GroupItemInDisplay || item is Skill;
        public static StackTracker ToStackable(this Item item, CharacterInventory inventory)
        {
            if (IsStackable(item))
            {
                return new StackTracker(item, inventory);
            }
            return null;
        }
    }
}
