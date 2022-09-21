using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;

namespace Assets.Testing
{
    internal class TestSlotData : ISlotData
    {
        public int SlotIndex { get; set; }
        public IActionSlotConfig Config { get; set; }
    }
}
