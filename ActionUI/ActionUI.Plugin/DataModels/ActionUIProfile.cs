using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Newtonsoft.Json;

namespace ModifAmorphic.Outward.UI.DataModels
{
    public class ActionUIProfile : IActionUIProfile
    {
        public string Name { get; set; }

        public bool ActionSlotsEnabled { get; set; }

        public bool DurabilityDisplayEnabled { get; set; }
        public bool StashCraftingEnabled { get; set; }

        [JsonIgnore]
        public string Path { get; set; }
    }
}
