using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.DataModels
{
    internal class HotbarSlotData : IHotbarSlotData
    {
        public int HotbarIndex { get; set; }

        [JsonProperty(ItemConverterType = typeof(ConcreteTypeConverter<SlotData>))]
        public List<ISlotData> Slots { get; set; } = new List<ISlotData>();
    }
}
