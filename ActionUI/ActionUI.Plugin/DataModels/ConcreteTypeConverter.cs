﻿using Newtonsoft.Json;
using System;

namespace ModifAmorphic.Outward.ActionUI.DataModels
{
    public class ConcreteTypeConverter<TConcrete> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => serializer.Deserialize<TConcrete>(reader);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer.Serialize(writer, value);
    }
}
