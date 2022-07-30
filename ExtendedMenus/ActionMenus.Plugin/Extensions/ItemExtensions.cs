using ModifAmorphic.Outward.ActionMenus.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Extensions
{
    internal static class ItemExtensions
    {
        //public static ItemSlotAction ToItemSlotAction(this Item item, Character character)
        //{
        //    var slotAction = new ItemSlotAction(actionName, item, _player, _characterUI.TargetCharacter, _getLogger)
        //    {
        //        ActionIcon = item.ItemIcon,
        //        Cooldown = new ItemCooldown(item),
        //        Stack = new Stackable(),
        //        //TargetAction = () => Logger.LogInfo($"Action {actionName} triggered!")
        //    };
        //    return slotAction;
        //}
        public static bool IsStackable(this Item item) => item.IsStackable || item.HasMultipleUses || item.GroupItemInDisplay || item is Skill;
        public static Stackable ToStackable(this Item item, CharacterInventory inventory)
        {
            if (IsStackable(item))
            {
                return new Stackable(item, inventory);
            }
            return null;
        }
    }
}
