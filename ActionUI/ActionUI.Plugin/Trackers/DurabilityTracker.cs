using ModifAmorphic.Outward.UI.Extensions;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.UI.Services
{
    internal class DurabilityTracker : IDurability
    {
        private readonly Equipment _equipment;

        private readonly static Color Grey = new Color(.3f, .3f, .3f, .8f);
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

        public DurableEquipmentType DurableEquipmentType => _equipment.GetDurableEquipmentType();

        public DurableEquipmentSlot DurableEquipmentSlot => _equipment.CurrentEquipmentSlot.SlotType.ToDurableEquipmentSlot();

        public float MinimumDisplayValue => .499f;

        public DurabilityTracker(Equipment equipment) => (_equipment) = (equipment);

        public float GetDurabilityRatio() => _equipment.DurabilityRatio;
    }
}
