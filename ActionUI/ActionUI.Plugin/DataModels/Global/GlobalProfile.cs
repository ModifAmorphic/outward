using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.DataModels.Global
{
    public class GlobalProfile
    {
        public SortedDictionary<int, CharacterEquipmentSet> CharacterEquipmentSets { get; set; } = new SortedDictionary<int, CharacterEquipmentSet>();
    }
}
