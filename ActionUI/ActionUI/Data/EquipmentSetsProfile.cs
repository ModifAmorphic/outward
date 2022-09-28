using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public class EquipmentSetsProfile<T> where T : IEquipmentSet
    {
        public List<T> EquipmentSets { get; set; } = new List<T>();
    }
}
