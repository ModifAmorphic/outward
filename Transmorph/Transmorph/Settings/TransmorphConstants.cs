using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModifAmorphic.Outward.Transmorph.Settings
{
    internal static class TransmorphConstants
    {
        public const int TransmogRecipeStartID = -1303000000;
        public const int TransmogTagWeaponStartUID = -1302990000;
        public const int TransmogTagArmorStartUID = -1302500000;
        public const int TransmogPrefix = -1356830026;
        public const int TransmogSecondaryItemID = 6300030;

        public static byte[] TransmorgBytePrefix = BitConverter.GetBytes(TransmogPrefix);

        public static TagSourceSelector TransmogTagSelector = new TagSourceSelector(
            new Tag("Axfc-kYcGEOguAqCUHh_fg", "transmog"));


        public static List<(int ItemID, Type EquipmentType)> StartingTransmogItemIDs = new List<(int ItemID, Type EquipmentType)>()
        {
                (2000061, typeof(Weapon)),      //Gold Machete
                //(2000031, typeof(Weapon)),      //Radiant Wolf Sword
                //(2000150, typeof(Weapon)),      //Brand
                //(2110215, typeof(Weapon)),      //Meteoric  Greataxe
                //(3000035, typeof(Armor)),       //Brigand Armor
                //(3100080, typeof(Armor)),       //Blue Sand Armor
                //(3100081, typeof(Armor)),       //Blue Sand Helm
                //(3100060, typeof(Armor)),       //Palladium Armor 
                //(3100191, typeof(Armor)),       //Master Kazite Oni Mask 
        };
    }
}
