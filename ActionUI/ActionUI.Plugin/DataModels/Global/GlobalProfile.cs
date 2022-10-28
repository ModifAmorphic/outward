using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.DataModels.Global
{
    public class GlobalProfile
    {
        public Dictionary<int, CharacterEquipmentSet> CharacterEquipmentSets { get; set; } = new Dictionary<int, CharacterEquipmentSet>();
        public Dictionary<int, string> CharacterItemIDs { get; set; } = new Dictionary<int, string>();
    }
}
