using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI.DataModels.Global
{
    public enum EquipmentSetTypes
    {
        Armor,
        Weapon,
    }

    public class CharacterEquipmentSet
    {
        public int SetID { get; set; }
        public string CharacterUID { get; set; }
        public string Name { get; set; }
        public EquipmentSetTypes EquipmentSetType { get; set; }
    }
}
