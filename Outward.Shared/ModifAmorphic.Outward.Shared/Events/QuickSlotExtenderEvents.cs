using ModifAmorphic.Outward.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward.Events
{
    static class QuickSlotExtenderEvents
    {
        public static event EventHandler<QuickSlotExtendedArgs> SlotsChanged;

        public static void RaiseSlotsChanged(object sender, QuickSlotExtendedArgs quickSlotExtendedArgs)
        {
            SlotsChanged?.Invoke(sender, quickSlotExtendedArgs);
        }

    }
}
