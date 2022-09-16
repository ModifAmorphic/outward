using ModifAmorphic.Outward.UI.Models;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Newtonsoft.Json;

namespace ModifAmorphic.Outward.UI.DataModels
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
