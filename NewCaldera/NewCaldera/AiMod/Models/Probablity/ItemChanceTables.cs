using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity
{
    internal class ItemChanceTables
    {
        public Dictionary<string, ConditionalChanceTable> ChanceTables { get; set; }
        public Dictionary<string, ItemQuantityChance> ItemQuantityChances { get; set; }
    }
}
