using BepInEx;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Settings
{
    internal class TransmogSettings
    {
        public const int RecipeStartID = -1303000000;
        public const int WeaponTagStartUID = -1302990000;
        public const int ArmorTagStartUID = -1302500000;
        public const int ItemPrefixUID = -1356830026;
        public const string ItemStringPrefixUID = "tmogr";

        public static readonly string PluginPath = Path.GetDirectoryName(TransmorphPlugin.Instance.Info.Location);
        public static readonly string ItemIconImageFilePath = Path.Combine(
                                            PluginPath,
                                            Path.Combine("assets","tex_men_transmogItem.png"));

        public static MenuIcons TransmogMenuIcons = new MenuIcons()
        {
            UnpressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsUnpressedTransmogrify.png")) },
            HoverIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsHoverTransmogrify.png")) },
            PressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsPressedTransmogrify.png")) }
        };

        public const string ItemIconName = "transmog";

        public static byte[] BytePrefixUID = BitConverter.GetBytes(ItemPrefixUID);

        public static TagSourceSelector TagSelector = new TagSourceSelector(
            new Tag("Axfc-kYcGEOguAqCUHh_fg", "transmog"));

           
        public static MenuIngredientFilters MenuFilters = new MenuIngredientFilters()
        {
            //BaseInventoryFilterTag = new Tag("70", "Item"),
            AdditionalInventoryIngredientFilter = null,
            EquippedIngredientFilter = new AvailableIngredientFilter()
            {
                EnchantFilter = AvailableIngredientFilter.FilterLogic.IncludeItems,
                ItemTypes = new HashSet<Type>() { typeof(Equipment) }
            },
        };

        public static class RemoveRecipe
        {
            public static string UID => "APpxHqi-NE2vG_MISt7C0Q";
            public static int RecipeID => (-1303000000);
            public static string RecipeName => "Remove Tranmogrify";
            public static int SecondIngredientID => 4300190; //Hex Potion
            private readonly static Tag transmogTag;
            public static Tag TransmogTag => transmogTag;
            public static string IconFile => Path.Combine(PluginPath, Path.Combine("assets", "tex_men_transmogRemoverArmor.png"));
            public static int SourceResultItemID => 3000136;
            public static int ResultItemID => -1303000001;
            public const string ResultItemName = "- Remove Transmog";
            public const string ResultItemDesc = "Recreated Item with Transmogrify removed. Enchantments are preserved.";

            static RemoveRecipe()
            {
                transmogTag = new Tag("-1303000000", "Transmog");
                transmogTag.SetTagType(Tag.TagTypes.Custom);
            }
        }

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

        public static HashSet<int> ExcludedItemIDs =
            new HashSet<int>()
            {
                -1301000,  //Stash Packs
                -1301002,
                -1301004,
                -1301006,
                -1301008,
                -1301010, //End Stash packs
            };

        public event Action<bool> TransmogMenuEnabledChanged;
        private bool _transmogMenuEnabled;
        public bool TransmogMenuEnabled
        {
            get => _transmogMenuEnabled;
            set
            {
                var oldValue = _transmogMenuEnabled;
                _transmogMenuEnabled = value;
                if (oldValue != _transmogMenuEnabled)
                    TransmogMenuEnabledChanged?.Invoke(_transmogMenuEnabled);
            }
        }

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

        public event Action<int> TransmogRecipeSecondaryItemIDChanged;
        private int _transmogRecipeSecondaryItemID;
        public int TransmogRecipeSecondaryItemID
        {
            get => _transmogRecipeSecondaryItemID;
            set
            {
                var oldValue = _transmogRecipeSecondaryItemID;
                _transmogRecipeSecondaryItemID = value;
                if (oldValue != _transmogRecipeSecondaryItemID)
                    TransmogRecipeSecondaryItemIDChanged?.Invoke(_transmogRecipeSecondaryItemID);
            }
        }
    }
}
