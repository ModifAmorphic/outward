using ModifAmorphic.Outward.NewCaldera.AiMod.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.Conditions
{
    internal class UniquesKilledCondition : Condition
    {
        public override string Name => nameof(ConditionTypes.UniquesKilled);

        public const string UniquesKilledKey = "UniquesKilled";
    }
}
