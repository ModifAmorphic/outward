using ModifAmorphic.Outward.ActionMenus.Settings;
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
        public bool CombatMode { get; set; } = true;

        [JsonProperty(ItemConverterType = typeof(ConcreteTypeConverter<HotbarData>))]
        public List<IHotbarSlotData> Hotbars { get; set; }
        public string NextHotkey { get; set; }
        public string NextRewiredActionName { get; set; } = RewiredConstants.ActionSlots.NextHotbarAction.name;
        public int NextRewiredActionId { get; set; } = RewiredConstants.ActionSlots.NextHotbarAction.id;
        public string PrevHotkey { get; set; }
        public string PrevRewiredActionName { get; set; } = RewiredConstants.ActionSlots.PreviousHotbarAction.name;
        public int PrevRewiredActionId { get; set; } = RewiredConstants.ActionSlots.PreviousHotbarAction.id;
    }
}
