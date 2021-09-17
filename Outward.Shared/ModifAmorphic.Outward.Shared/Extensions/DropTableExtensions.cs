using ModifAmorphic.Outward.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Extensions
{
    public static class DropTableExtensions
    {
        public static List<GuaranteedDrop> GetAllGuaranteedDrops(this Dropable dropable) => 
            dropable.GetPrivateField<Dropable, List<GuaranteedDrop>>("m_allGuaranteedDrops");

        public static List<BasicItemDrop> GetItemDrops(this GuaranteedDrop guaranteedDrop) =>
            guaranteedDrop.GetPrivateField<GuaranteedDrop, List<BasicItemDrop>>("m_itemDrops");
        public static void SetItemDrops(this GuaranteedDrop guaranteedDrop, List<BasicItemDrop> itemDrops) =>
            guaranteedDrop.SetPrivateField("m_itemDrops", itemDrops);

        public static List<BasicItemDrop> ToItemDrops(this IEnumerable<CustomGuaranteedDrop> customGuaranteedDrops) =>
            customGuaranteedDrops.Select(d => new BasicItemDrop()
            {
                ItemID = d.ItemID,
                MaxDropCount = d.MaxAmount,
                MinDropCount = d.MinAmount
            }).ToList();

    }
}
