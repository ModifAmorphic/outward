using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

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
