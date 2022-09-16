using ModifAmorphic.Outward.Unity.ActionMenus;

namespace ModifAmorphic.Outward.UI.Models
{
    internal class ActionConfig : IActionSlotConfig
    {
        public string HotkeyText { get; set; }
        public bool ShowCooldownTime { get; set; }
        public bool PreciseCooldownTime { get; set; }
        public bool ShowZeroStackAmount { get; set; }
        public EmptySlotOptions EmptySlotOption { get; set; }
        public string RewiredActionName { get; set; }
        public int RewiredActionId { get; set; }
    }
}
