using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI.Extensions
{
    internal static class EquipmentSetExtensions
    {
        public static IEnumerable<Skill.ItemRequired> ToItemsRequired(this IEquipmentSet set)
        {
            return set.GetEquipSlots().Where(s => s != null).Select(s => new Skill.ItemRequired()
            { 
                Consume = false,
                Quantity = 1,
                Item = ItemManager.Instance.GetItem(s.UID)
            }
            );
        }
    }
}
