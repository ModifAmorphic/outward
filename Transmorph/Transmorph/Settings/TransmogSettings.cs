using BepInEx;
using ModifAmorphic.Outward.Transmorph.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorph.Settings
{
    internal class TransmogSettings
    {
        public const int RecipeStartID = -1303000000;
        public const int WeaponTagStartUID = -1302990000;
        public const int ArmorTagStartUID = -1302500000;
        public const int ItemPrefixUID = -1356830026;
        public const string ItemStringPrefixUID = "tmogr";
        public const int RecipeSecondaryItemID = 6300030;
        public static readonly string IconImageFilePath = Path.Combine(
                                            Path.GetDirectoryName(TransmorphPlugin.Instance.Info.Location),
                                            "tex_men_transmogItem.png");
        public const string IconName = "transmog";

        public static byte[] BytePrefixUID = BitConverter.GetBytes(ItemPrefixUID);

        public static TagSourceSelector TagSelector = new TagSourceSelector(
            new Tag("Axfc-kYcGEOguAqCUHh_fg", "transmog"));


        public static Dictionary<int, UID> StartingTransmogRecipes = new Dictionary<int, UID>()
        {
            { 2000061, "JrwXy-dujEujEGPU3BB9ow" },      //Gold Machete
                //(2000031, typeof(Weapon)),      //Radiant Wolf Sword
                //(2000150, typeof(Weapon)),      //Brand
                //(2110215, typeof(Weapon)),      //Meteoric  Greataxe
                //(3000035, typeof(Armor)),       //Brigand Armor
                //(3100080, typeof(Armor)),       //Blue Sand Armor
                //(3100081, typeof(Armor)),       //Blue Sand Helm
                //(3100060, typeof(Armor)),       //Palladium Armor 
                //(3100191, typeof(Armor)),       //Master Kazite Oni Mask 
        };

        public event Action AllCharactersLearnRecipesEnabled;
        private bool _allCharactersLearnRecipes;
        public bool AllCharactersLearnRecipes {
            get => _allCharactersLearnRecipes;
            set
            {
                _allCharactersLearnRecipes = value;
                if (_allCharactersLearnRecipes)
                    AllCharactersLearnRecipesEnabled?.Invoke();
            } 
        }
        
    }
}
