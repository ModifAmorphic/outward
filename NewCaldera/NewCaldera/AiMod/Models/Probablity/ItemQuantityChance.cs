using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity
{
    internal class ItemQuantityChance
    {
        public string Name { get; set; }
        public int ItemID { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public decimal QuantityChance { get; set; }
    }
}
