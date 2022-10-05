using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets
{
    public interface IEquipmentSet
    {
        string Name { get; set; }
        EquipSlots IconSlot { get; set; }
        int SetID { get; set; }

        IEnumerable<EquipSlot> GetEquipSlots();
        EquipSlot GetIconEquipSlot();
    }
}
