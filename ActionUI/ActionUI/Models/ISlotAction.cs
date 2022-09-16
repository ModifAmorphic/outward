using System;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface ISlotAction
    {
        string DisplayName { get; }
        ICooldown Cooldown { get; }
        IStackable Stack { get; }
        Dictionary<BarPositions, IBarProgress> ActiveBars { get; }
        bool HasDynamicIcon { get; }
        ActionSlotIcon[] ActionIcons { get; }
        bool CheckOnUpdate { get; }
        ActionSlotIcon[] GetDynamicIcons();
        bool GetIsActionRequested();
        bool GetEnabled();
        bool GetIsEditRequested();
        void SlotActionAssigned(ActionSlot assignedSlot);
        void SlotActionUnassigned();
        Action TargetAction { get; }

        event Action OnActionRequested;
        event Action OnEditRequested;
        event Action<ActionSlotIcon[]> OnIconsChanged;
    }
}
