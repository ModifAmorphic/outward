using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.Unity.ActionUI;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class DurabilityActionSlotTracker : IBarProgress
    {
        private readonly EquipmentSlotAction _equipSlotAction;

        private readonly BarPositions _barPosition;
        public BarPositions BarPosition => _barPosition;

        private float _lastDurability = -1f;

        public bool IsEnabled { get; internal set; }

        private readonly static Color Grey = new Color(.5f, .5f, .5f, 1f);
        private readonly static Color Yellow = new Color(0.7830189f, 0.7024478f, 0.04924639f, 1f);
        private readonly static Color Orange = new Color(0.9433962f, 0.5298792f, 0.08603289f, 1f);
        private readonly static Color Red = new Color(0.7169812f, 0.08259042f, 0.03832913f, 1f);

        List<ColorRange> colorRanges = new List<ColorRange>()
        {
            new ColorRange() { Min = .5f, Max = 1f, Color = Grey },
            new ColorRange() { Min = .25f, Max = .499f, Color = Yellow },
            new ColorRange() { Min = .000f, Max = .249f, Color = Orange },
            new ColorRange() { Min = .000f, Max = .000f, Color = Red },
        };
        public List<ColorRange> ColorRanges => colorRanges;

        public DurabilityActionSlotTracker(EquipmentSlotAction equipSlotAction, BarPositions barPosition) => (_equipSlotAction, _barPosition, IsEnabled) = (equipSlotAction, barPosition, true);

        public float GetProgress()
        {
            if (!string.IsNullOrEmpty(_equipSlotAction.ActionEquipment.UID))
            {
                if (_equipSlotAction.ActionEquipment.CurrentDurability != _lastDurability)
                {
                    _lastDurability = _equipSlotAction.ActionEquipment.CurrentDurability;
                    _equipSlotAction.DurabilityChanged(_equipSlotAction.ActionEquipment.CurrentDurability);
                }
                return IsEnabled ? _equipSlotAction.ActionEquipment.DurabilityRatio : 0f;
            }
            else //If prefab, always return full durability
            {
                return IsEnabled ? 1f : 0f;
            }
        }


    }
}
