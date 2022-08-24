using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Settings
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
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[8].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[8].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 1,
                        Config = new ActionConfig() {
                            HotkeyText = "E",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[9].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[9].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 2,
                        Config = new ActionConfig() {
                            HotkeyText = "R",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[10].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[10].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 3,
                        Config = new ActionConfig() { 
                            HotkeyText = "1",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[0].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[0].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 4,
                        Config = new ActionConfig() {
                            HotkeyText = "2",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[1].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[1].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 5,
                        Config = new ActionConfig() {
                            HotkeyText = "3",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[2].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[2].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 6,
                        Config = new ActionConfig() {
                            HotkeyText = "4",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[3].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[3].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 7,
                        Config = new ActionConfig() {
                            HotkeyText = "5",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[4].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[4].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 8,
                        Config = new ActionConfig() {
                            HotkeyText = "6",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[5].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[5].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 9,
                        Config = new ActionConfig() {
                            HotkeyText = "7",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[6].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[6].id,
                        }
                    },
                    new SlotData()
                    {
                        SlotIndex = 10,
                        Config = new ActionConfig() {
                            HotkeyText = "8",
                            RewiredActionName = RewiredConstants.ActionSlots.Actions[7].name,
                            RewiredActionId = RewiredConstants.ActionSlots.Actions[7].id,
                        }
                    },
                }
            }
        };

        internal static ProfileData DefaultProfile = new ProfileData()
        {
            Name = "Profile 1",
            Rows = 1,
            SlotsPerRow = hotbarAssignments.First().Slots.Count,
            Hotbars = hotbarAssignments,
            CombatMode = true,
            NextRewiredActionId = RewiredConstants.ActionSlots.NextHotbarAction.id,
            NextRewiredActionName = RewiredConstants.ActionSlots.NextHotbarAction.name,
            PrevRewiredActionId = RewiredConstants.ActionSlots.PreviousHotbarAction.id,
            PrevRewiredActionName = RewiredConstants.ActionSlots.PreviousHotbarAction.name,
        };

        public event Action<int> HotbarsChanged;
        private int _hotbars;
        public int Hotbars
        {
            get => _hotbars;
            set
            {
                var oldValue = _hotbars;
                _hotbars = value;
                if (oldValue != _hotbars)
                    HotbarsChanged?.Invoke(_hotbars);
            }
        }

        public event Action<int> ActionSlotsChanged;
        private int _actionSlots;
        public int ActionSlots
        {
            get => _actionSlots;
            set
            {
                var oldValue = _actionSlots;
                _actionSlots = value;
                if (oldValue != _actionSlots)
                    ActionSlotsChanged?.Invoke(_actionSlots);
            }
        }
    }
}
