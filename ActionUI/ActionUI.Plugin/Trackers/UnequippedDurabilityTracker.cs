using ModifAmorphic.Outward.Unity.ActionMenus;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.UI.Services
{
    internal class UnequippedDurabilityTracker : IDurability
    {
        private readonly static Color Grey = new Color(.3f, .3f, .3f, .75f);
        private readonly static Color Yellow = new Color(0.7830189f, 0.7024478f, 0.04924639f, .8f);
        private readonly static Color Orange = new Color(0.9433962f, 0.5298792f, 0.08603289f, .8f);
        private readonly static Color Red = new Color(0.7169812f, 0.08259042f, 0.03832913f, .9f);

        List<ColorRange> colorRanges = new List<ColorRange>()
        {
            new ColorRange() { Min = .5f, Max = 1f, Color = Grey },
            new ColorRange() { Min = .25f, Max = .499f, Color = Yellow },
            new ColorRange() { Min = .000f, Max = .249f, Color = Orange },
            new ColorRange() { Min = .000f, Max = .000f, Color = Red },
        };
        public List<ColorRange> ColorRanges => colorRanges;

        private readonly DurableEquipmentType _equipmentType;
        public DurableEquipmentType DurableEquipmentType => _equipmentType;

        private readonly DurableEquipmentSlot _slot;
        public DurableEquipmentSlot DurableEquipmentSlot => _slot;

        public float MinimumDisplayValue => .0f;

        public UnequippedDurabilityTracker(DurableEquipmentSlot slot, DurableEquipmentType equipmentType, float durabilityRatio) => (_slot, _equipmentType, _durabilityRatio) = (slot, equipmentType, durabilityRatio);

        private readonly float _durabilityRatio;
        public float GetDurabilityRatio() => _durabilityRatio;
    }
}
