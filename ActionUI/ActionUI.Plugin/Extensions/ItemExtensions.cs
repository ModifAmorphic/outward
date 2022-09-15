using ModifAmorphic.Outward.UI.Models;

namespace ModifAmorphic.Outward.UI.Extensions
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
