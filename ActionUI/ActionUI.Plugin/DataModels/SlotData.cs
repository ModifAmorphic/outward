using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.DataModels
{
    internal class SlotData : ISlotData
    {
        public int SlotIndex { get; set; }
        public int ItemID { get; set; } = -1;
        public string ItemUID { get; set; }

        [JsonConverter(typeof(ConcreteTypeConverter<ActionConfig>))]
        public IActionSlotConfig Config { get; set; }
    }
}
