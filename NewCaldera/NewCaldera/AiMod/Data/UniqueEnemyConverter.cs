using ModifAmorphic.Outward.NewCaldera.AiMod.Models;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Data
{
    public class UniqueEnemyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var unique = serializer.Deserialize<UniqueEnemy>(reader);
            foreach(var table in unique.DropTables)
            {
                foreach (var condition in table.Value.DropConditions)
                {
                    var uniqueProps = condition.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(p => p.GetCustomAttributes(typeof(UniquePropertyAttribute), false).Any());
                    foreach (var uniqueProp in uniqueProps)
                    {
                        var uniqueAttribute = uniqueProp.GetCustomAttributes(typeof(UniquePropertyAttribute), false).Cast<UniquePropertyAttribute>().First();
                        SetProperty(uniqueAttribute.UniquePropertyType, uniqueProp, condition, unique);
                    }
                }
            }

            return unique;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer.Serialize(writer, value);

        private void SetProperty(UniquePropertyTypes propertyType, PropertyInfo targetProperty, object targetClass, UniqueEnemy uniqueEnemy)
        {
            if (propertyType == UniquePropertyTypes.UID)
            {
                targetProperty.SetValue(targetClass, uniqueEnemy.UID);
            }
        }
    }
}
