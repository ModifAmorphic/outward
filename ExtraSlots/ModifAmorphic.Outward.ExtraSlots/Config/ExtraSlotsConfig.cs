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
        private int extraQuickSlots = 2;
        public int ExtraQuickSlots
        {
            get { return extraQuickSlots; }
            set
            {
                extraQuickSlots = value;
            }
        }

        private string extraQuickSlotMenuText = "Ex Quickslot {ExtraSlotNumber}";
        public string ExtraQuickSlotMenuText
        {
            get { return extraQuickSlotMenuText; }
            set
            {
                extraQuickSlotMenuText = value;
            }
        }

        private bool centerQuickSlotPanel = false;
        public bool CenterQuickSlotPanel
        {
            get { return centerQuickSlotPanel; }
            set
            {
                centerQuickSlotPanel = value;
            }
        }

        private QuickSlotBarAlignmentOptions quickSlotBarAlignmentOption = QuickSlotBarAlignmentOptions.None;
        public QuickSlotBarAlignmentOptions ExtraSlotsAlignmentOption
        {
            get { return quickSlotBarAlignmentOption; }
            set
            {
                quickSlotBarAlignmentOption = value;
            }
        }

        private float quickSlotXOffset = 0f;
        public float QuickSlotXOffset
        {
            get { return quickSlotXOffset; }
            set
            {
                quickSlotXOffset = value;
            }
        }

        private LogLevel logLevel = LogLevel.Info;
        public LogLevel LogLevel
        {
            get { return logLevel; }
            set
            {
                logLevel = value;
            }
        }
        private float moveStabilityBarUp_Y_Offset = -40f;
        public float MoveStabilityBarUp_Y_Offset
        {
            get { return moveStabilityBarUp_Y_Offset; }
            set
            {
                moveStabilityBarUp_Y_Offset = value;
            }
        }

        private int internalQuickSlotStartingId = 12;
        public int InternalQuickSlotStartingId
        {
            get { return internalQuickSlotStartingId; }
            set
            {
                internalQuickSlotStartingId = value;
            }
        }
    }
}
