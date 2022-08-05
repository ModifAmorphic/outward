using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.DataModels
{
    internal class ProfileData : IHotbarProfileData
    {
        public string Name { get; set; }
        public int Rows { get; set; }
        public int SlotsPerRow { get; set; }

        [JsonProperty(ItemConverterType = typeof(ConcreteTypeConverter<HotbarSlotData>))]
        public List<IHotbarSlotData> Hotbars { get; set; }
    }
}
