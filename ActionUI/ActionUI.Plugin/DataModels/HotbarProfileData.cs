using ModifAmorphic.Outward.UI.Settings;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.UI.DataModels
{
    internal class HotbarProfileData : IHotbarProfile
    {
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
