using System.Collections.Generic;

namespace ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings
{
    class QuickSlotExtendedArgs //: EventArgs
    {
        public int StartId { get; }
        public IEnumerable<ExtendedQuickSlot> ExtendedQuickSlots { get; }

        public QuickSlotExtendedArgs(int startId, IEnumerable<ExtendedQuickSlot> extendedQuickSlots) => (StartId, ExtendedQuickSlots) = (startId, extendedQuickSlots);
    }
}
