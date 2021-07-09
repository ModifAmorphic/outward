using ModifAmorphic.Outward.Models;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Events
{
    class QuickSlotExtendedArgs //: EventArgs
    {
        public int StartId { get; }
        public IEnumerable<ExtendedQuickSlot> ExtendedQuickSlots { get; }

        public QuickSlotExtendedArgs(int startId, IEnumerable<ExtendedQuickSlot> extendedQuickSlots) => (StartId, ExtendedQuickSlots) = (startId, extendedQuickSlots);
    }
}
