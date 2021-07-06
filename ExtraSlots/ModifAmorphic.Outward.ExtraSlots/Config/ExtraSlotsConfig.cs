using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifAmorphic.Outward.ExtraSlots.Config
{
    public class ExtraSlotsConfig
    {
        public int ExtraQuickSlots { get; set; } = 3;

        //UI Alignment Settings
        public bool CenterQuickSlotPanel { get; set; } = false;
        public QuickSlotBarAlignmentOptions QuickSlotBarAlignmentOption { get; set; } = QuickSlotBarAlignmentOptions.None;
        public float CenterQuickSlot_X_Offset { get; set; } = 0f;
        public float MoveStabilityBarUp_Y_Offset { get; set; } = -40f;
        public float MoveQuickSlotBarUp_Y_Offset { get; set; } = 37f;
        
        //Internal Settings
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public string ExtraQuickSlotMenuText { get; set; } = "Ex Quickslot {ExtraSlotNumber}";
    }
}
