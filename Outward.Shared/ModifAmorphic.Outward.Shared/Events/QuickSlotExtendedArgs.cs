using ModifAmorphic.Outward.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward.Events
{
    class QuickSlotExtendedArgs //: EventArgs
    {
        public int StartId { get; }
        public IEnumerable<ExtendedQuickSlot> ExtendedQuickSlots { get; }

        public QuickSlotExtendedArgs(int startId, IEnumerable<ExtendedQuickSlot> extendedQuickSlots) => (StartId, ExtendedQuickSlots) = (startId, extendedQuickSlots);
    }
}
