using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Testing
{
    internal class TestDefaultProfile
    {
        private static List<IHotbarSlotData> hotbarAssignments = new List<IHotbarSlotData>()
        {
            new TestHotbarData()
            {
                HotbarIndex = 0,
                
                Slots = new List<ISlotData>()
                {
                    new TestSlotData()
                    {
                        SlotIndex = 0,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "1",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 1,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "2",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 2,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "3",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 3,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "4",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 4,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "5",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 5,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "6",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 6,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "7",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 7,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "8",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 8,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "Q",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 9,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "E",
                        }
                    },
                    new TestSlotData()
                    {
                        SlotIndex = 10,
                        Config = new TestActionSlotConfig() {
                            HotkeyText = "R",
                        }
                    }
                }
            }
        };

        internal static TestHotbarProfileData DefaultProfile = new TestHotbarProfileData()
        {
            Name = "Profile 1",
            Rows = 1,
            SlotsPerRow = hotbarAssignments.First().Slots.Count,
            Hotbars = hotbarAssignments
        };
    }
}
