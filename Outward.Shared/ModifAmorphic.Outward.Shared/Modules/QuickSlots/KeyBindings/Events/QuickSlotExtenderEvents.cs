using System;

namespace ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings
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
