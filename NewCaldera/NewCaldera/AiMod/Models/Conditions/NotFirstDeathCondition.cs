using ModifAmorphic.Outward.NewCaldera.AiMod.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.Conditions
{
    internal class NotFirstDeathCondition : Condition
    {
        public override string Name => nameof(ConditionTypes.NotFirstDeath);

        //[JsonIgnore]
        //[UniqueProperty(UniquePropertyType = UniquePropertyTypes.UID)]
        //public string UniqueUID { get; set; }

        //[JsonIgnore]
        //public RegionCyclesData RegionCyclesData { get; set; }

        //public override bool IsConditionMet()
        //{
        //    if (string.IsNullOrWhiteSpace(UniqueUID))
        //        throw new InvalidOperationException($"Condition cannot be analyized before property {nameof(UniqueUID)} has been set.");

        //    if (RegionCyclesData == null)
        //        throw new InvalidOperationException($"Condition cannot be analyized before property {nameof(RegionCyclesData)} has been set.");

        //    if (RegionCyclesData.TryGetUnique(UniqueUID, out var unique))
        //    {
        //        return unique.PreviousDeaths.Count > 1;
        //    }
        //    return false;
        //}
    }
}
