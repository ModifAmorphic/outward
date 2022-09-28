using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets
{
    public interface IEquipmentSet
    {
        string Name { get; set; }
        EquipSlots SlotIcon { get; set; }
        int SetID { get; set; }

        IEnumerable<EquipSlot> GetEquipSlots();
        EquipSlot GetIconEquipSlot();
    }
}
