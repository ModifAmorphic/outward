using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Conditions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity
{
    internal class ConditionalChanceTable
    {
        public string Name { get; set; }
        public int MinItemTables { get; set; }
        public int MaxItemTables { get; set; }
        public decimal QuantityChance { get; set; }
        public Dictionary<string, decimal> WeightedItemTables { get; set; }
        [JsonIgnore]
        public Dictionary<string, ItemQuantityChance> ItemTables { get; set; }
        
        [JsonProperty(ItemConverterType = typeof(JsonConditionConverter))]
        public List<Condition> DropConditions { get; set; }
    }
}
