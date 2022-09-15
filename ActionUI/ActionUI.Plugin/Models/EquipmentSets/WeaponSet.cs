using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UI.Models.EquipmentSets
{
    internal class WeaponSet
    {
        public string Name { get; set; }
        public string SlotIconPath { get; set; }
        public EquipmentSlot LeftHand { get; set; }
        public EquipmentSlot RightHand { get; set; }
    }
}
