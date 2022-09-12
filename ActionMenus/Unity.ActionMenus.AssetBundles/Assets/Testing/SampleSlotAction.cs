using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Testing
{
    internal class SampleSlotAction : ISlotAction
    {
        public ICooldown Cooldown { get; set; }

        public IStackable Stack { get; set; }

        public Sprite ActionIcon { get; set; }

        public bool CheckOnUpdate => false;

        public Action TargetAction => () => { };

        public string DisplayName { get; set; }

        public bool HasDynamicIcon { get; set; }

        public ActionSlotIcon[] ActionIcons { get; set; }


        public Dictionary<BarPositions, IBarProgress> ActiveBars => null;

        public event Action OnActionRequested;
        public event Action OnEditRequested;
        public event Action<ActionSlotIcon[]> OnIconsChanged;

        public bool GetIsActionRequested()
        {
            return false;
        }

        public bool GetEnabled()
        {
            return true;
        }

        public bool GetIsEditRequested()
        {
            throw new NotImplementedException();
        }
        public void RequestEdit() => OnEditRequested?.Invoke();

        public void SlotActionAssigned(ActionSlot assignedSlot) { }

        public void SlotActionUnassigned() { }

        public ActionSlotIcon[] GetDynamicIcons()
        {
            throw new NotImplementedException();
        }
    }
}
