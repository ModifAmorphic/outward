using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using ModifAmorphic.Outward.ActionMenus.Models;

namespace ModifAmorphic.Outward.ActionMenus.DataModels
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
