using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Newtonsoft.Json;

namespace ModifAmorphic.Outward.ActionUI.DataModels
{
    public class ActionUIProfile : IActionUIProfile
    {
        public string Name { get; set; }

        public bool ActionSlotsEnabled { get; set; }

        public bool DurabilityDisplayEnabled { get; set; }
        public bool EquipmentSetsEnabled { get; set; }
        public bool StashCraftingEnabled { get; set; }
        public bool CraftingOutsideTownEnabled { get; set; }
        public EquipmentSetsSettingsProfile EquipmentSetsSettingsProfile { get; set; }
        public string LastLoadedModVersion { get; set; }

        [JsonIgnore]
        public string Path { get; set; }

    }
}
