using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Logging;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class ChainedSlotAction : SlotActionBase
    {
        public SortedList<int, Item> ActionChain { get; internal set; }

        public ChainedSlotAction(Skill chainSkill, SortedList<int, Item> actionChain, Player player, Character character, SlotDataService slotData, bool combatModeEnabled, Func<IModifLogger> getLogger) : base(chainSkill, player, character, slotData, combatModeEnabled, getLogger)
        {
            ActionChain = actionChain;
        }

        public override void ActivateTarget()
        {
            throw new NotImplementedException();
        }

        public override bool GetEnabled()
        {
            throw new NotImplementedException();
        }

        protected override bool GetIsLocked()
        {
            throw new NotImplementedException();
        }
    }
}
