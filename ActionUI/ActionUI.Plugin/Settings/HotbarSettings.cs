using ModifAmorphic.Outward.ActionUI.DataModels;
using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.ActionUI.Settings
{
    public class HotbarSettings
    {
        private static List<IHotbarSlotData> hotbarAssignments = new List<IHotbarSlotData>()
        {
            new HotbarData()
            {
                HotbarIndex = 0,
                RewiredActionId =  RewiredConstants.ActionSlots.HotbarNavActions[0].id,
                RewiredActionName =  RewiredConstants.ActionSlots.HotbarNavActions[0].name,
                Slots = new List<ISlotData>()
                {
                    new SlotData()
                    {
                        SlotIndex = 0,
                        Config = new ActionConfig() {
                            HotkeyText = "Q",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[0].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[0].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 1,
                        Config = new ActionConfig() {
                            HotkeyText = "E",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[1].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[1].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 2,
                        Config = new ActionConfig() {
                            HotkeyText = "R",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[2].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[2].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 3,
                        Config = new ActionConfig() {
                            HotkeyText = "1",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[3].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[3].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 4,
                        Config = new ActionConfig() {
                            HotkeyText = "2",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[4].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[4].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 5,
                        Config = new ActionConfig() {
                            HotkeyText = "3",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[5].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[5].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 6,
                        Config = new ActionConfig() {
                            HotkeyText = "4",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[6].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[6].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 7,
                        Config = new ActionConfig() {
                            HotkeyText = "5",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[7].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[7].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 8,
                        Config = new ActionConfig() {
                            HotkeyText = "6",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[8].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[8].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 9,
                        Config = new ActionConfig() {
                            HotkeyText = "7",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[9].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[9].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 10,
                        Config = new ActionConfig() {
                            HotkeyText = "8",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[10].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[10].id,
                        }
                    },
                }
            }
        };

        internal static HotbarProfileData DefaulHotbarProfile = new HotbarProfileData()
        {
            Rows = 1,
            SlotsPerRow = hotbarAssignments.First().Slots.Count,
            Hotbars = hotbarAssignments,
            HideLeftNav = false,
            CombatMode = true,
            NextRewiredActionId = RewiredConstants.ActionSlots.NextHotbarAction.id,
            NextRewiredActionName = RewiredConstants.ActionSlots.NextHotbarAction.name,
            PrevRewiredActionId = RewiredConstants.ActionSlots.PreviousHotbarAction.id,
            PrevRewiredActionName = RewiredConstants.ActionSlots.PreviousHotbarAction.name,
        };

    }
}
