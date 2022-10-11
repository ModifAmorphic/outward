namespace ModifAmorphic.Outward.ActionUI.Extensions
{
    internal static class ItemExtensions
    {
        public static bool IsStackable(this Item item) => item.IsStackable || item.HasMultipleUses || item.GroupItemInDisplay || item is Skill;
    }
}
