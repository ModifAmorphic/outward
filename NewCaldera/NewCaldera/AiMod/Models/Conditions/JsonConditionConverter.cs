using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.Conditions
{
    internal class JsonConditionConverter : CustomCreationConverter<Condition>
    {
        private ConditionTypes _conditionType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.ReadFrom(reader);

            var name = jobj["Name"].ToString();
            _conditionType = (ConditionTypes)Enum.Parse(typeof(ConditionTypes), name);
            return base.ReadJson(jobj.CreateReader(), objectType, existingValue, serializer);
        }

        public override Condition Create(Type objectType)
        {
            switch (_conditionType)
            {
                case ConditionTypes.NotFirstDeath:
                    return new NotFirstDeathCondition();
                case ConditionTypes.UniquesKilled:
                    return new UniquesKilledCondition();
                case ConditionTypes.PlayerDeaths:
                    return new PlayerDeathsCondition();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
