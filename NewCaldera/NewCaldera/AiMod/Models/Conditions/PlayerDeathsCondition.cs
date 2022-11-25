using ModifAmorphic.Outward.NewCaldera.AiMod.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.Conditions
{
    internal class PlayerDeathsCondition : Condition
    {
        public override string Name => nameof(ConditionTypes.UniquesKilled);

        public const string PlayerDeathsKey = "PlayerDeathsMax";
    }
}
