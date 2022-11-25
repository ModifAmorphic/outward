using ModifAmorphic.Outward.NewCaldera.AiMod.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.Conditions
{
    internal abstract class Condition
    {
        public abstract string Name { get; }
        public virtual Dictionary<string, string> Conditions { get; set; }
        public virtual Dictionary<string, decimal> NumericConditions { get; set; }
    }
}
