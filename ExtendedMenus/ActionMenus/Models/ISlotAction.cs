using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface ISlotAction
    {
        string DisplayName { get; }
        ICooldown Cooldown { get; }
        IStackable Stack { get; }
        bool HasDynamicIcon { get; }
        Sprite ActionIcon { get; }
        bool CheckOnUpdate { get; }
        Sprite GetDynamicIcon();
        bool GetIsActionRequested();
        bool GetEnabled();
        bool GetIsEditRequested();
        void SlotActionAssigned(ActionSlot assignedSlot);
        void SlotActionUnassigned();
        Action TargetAction { get; }

        event Action OnActionRequested;
        event Action OnEditRequested;
    }
}
