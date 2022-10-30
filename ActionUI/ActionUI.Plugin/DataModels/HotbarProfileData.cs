using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace ModifAmorphic.Outward.ActionUI.DataModels
{
    internal class HotbarProfileData : IHotbarProfile
    {
        public int Rows { get; set; }
        public int SlotsPerRow { get; set; }
        public bool HideLeftNav { get; set; }
        public bool CombatMode { get; set; } = true;

        [JsonProperty(ItemConverterType = typeof(ConcreteTypeConverter<HotbarData>))]
        public List<IHotbarSlotData> Hotbars { get; set; }
        public string NextHotkey { get; set; }
        public string NextRewiredActionName { get; set; } = RewiredConstants.ActionSlots.NextHotbarAction.name;
        public int NextRewiredActionId { get; set; } = RewiredConstants.ActionSlots.NextHotbarAction.id;
        public string PrevHotkey { get; set; }
        public string PrevRewiredActionName { get; set; } = RewiredConstants.ActionSlots.PreviousHotbarAction.name;
        public int PrevRewiredActionId { get; set; } = RewiredConstants.ActionSlots.PreviousHotbarAction.id;
        public string NextRewiredAxisActionName { get; set; } = RewiredConstants.ActionSlots.NextHotbarAxisAction.name;
        public int NextRewiredAxisActionId { get; set; } = RewiredConstants.ActionSlots.NextHotbarAxisAction.id;
        public string PrevRewiredAxisActionName { get; set; } = RewiredConstants.ActionSlots.PreviousHotbarAxisAction.name;
        public int PrevRewiredAxisActionId { get; set; } = RewiredConstants.ActionSlots.PreviousHotbarAxisAction.id;

        [DefaultValue(100)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Scale { get; set; } = 100;
    }
}
