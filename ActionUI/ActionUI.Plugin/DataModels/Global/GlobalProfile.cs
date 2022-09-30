using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI.DataModels.Global
{
    public class GlobalProfile
    {
        public SortedDictionary<int, CharacterEquipmentSet> CharacterEquipmentSets { get; set; } = new SortedDictionary<int, CharacterEquipmentSet>();
    }
}
