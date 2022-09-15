using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UI.DataModels
{
    internal class HotbarData : IHotbarSlotData
    {
        public int HotbarIndex { get; set; }

        [JsonProperty(ItemConverterType = typeof(ConcreteTypeConverter<SlotData>))]
        public List<ISlotData> Slots { get; set; } = new List<ISlotData>();
        public string HotbarHotkey { get; set; }
        public string RewiredActionName { get; set; }
        public int RewiredActionId { get; set; }
    }
}
