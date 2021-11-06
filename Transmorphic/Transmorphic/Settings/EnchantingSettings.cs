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
    internal class EnchantingSettings
    {
        public const int RecipeStartID = -1303100000;
        public const int RecipeResultStartID = -1303100000;
                                                 //9400233
        public const int VirginHelmetID = 3100261;
        public const int VirginArmorID = 3100260;
        public const int VirginBootsID = 3100262;
        public const int IronSwordID = 2010000; //2000010;
        public const int SimpleBowID = 2200000;
        public const int FlintlockPistolID = 5110100;
        public const int RondelDaggerID = 5110000;
        public const int ChakramID = 5110030;
        public const int LexiconID = 5100500;
        public const int VirginLanternID = 5100100;
        public const int CompasswoodStaffID = 2150030;

        //public const int WeaponTagStartUID = -1302990000;
        //public const int ArmorTagStartUID = -1302500000;
        //public const int ItemPrefixUID = -1356830026;
        //public const string ItemStringPrefixUID = "tmogr";

        public static readonly string PluginPath = Path.GetDirectoryName(TransmorphPlugin.Instance.Info.Location);
        public static readonly string ItemIconImageFilePath = Path.Combine(
                                            PluginPath,
                                            Path.Combine("assets","tex_men_transmogItem.png"));

        public static MenuIcons EnchantMenuIcons = new MenuIcons()
        {
            UnpressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsUnpressedEnchanting.png")) },
            HoverIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsHoverEnchanting.png")) },
            PressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsPressedEnchanting.png")) }
        };

        public static RecipeSelectorDisplayConfig RecipeSelectorDisplayConfig = new RecipeSelectorDisplayConfig()
        {
            ExtraIngredientSlotOption = ExtraIngredientSlotOptions.Both
        };

        
        public static TagSourceSelector EquipmentTagSelector = new TagSourceSelector(ItemTags.EquipmentTag);
        //public static TagSourceSelector EnchantIngredientTag = new TagSourceSelector(TagSourceManager.Instance.GetTag("54"));
        public static MenuIngredientFilters MenuFilters = new MenuIngredientFilters()
        {
            BaseInventoryFilterTag = new TagSourceSelector(ItemTags.EnchantIngredientsTag),
            EquippedIngredientFilter = new AvailableIngredientFilter()
            {
                EnchantFilter = AvailableIngredientFilter.FilterLogic.ExcludeItems,
                ItemTypes = new HashSet<Type>() { typeof(Equipment) }
                //InventoryFilterTag = EquipmentTagSelector
            },
            AdditionalInventoryIngredientFilter = new AvailableIngredientFilter()
            {
                EnchantFilter = AvailableIngredientFilter.FilterLogic.ExcludeItems,
                SpecificItemFilter = AvailableIngredientFilter.FilterLogic.OnlyItems,
                SpecificItemIDs = new HashSet<int>() { PedestalItemID },
                //InventoryFilterTag = EnchantIngredientTag
                //ItemTypes = new HashSet<Type>() { typeof(Equipment) }
            }
        };
        public const int PedestalItemID = 5000200;
        public static Guid PedestalCorelationID = Guid.NewGuid();
        public static Guid EquipmentCorelationID = Guid.NewGuid();
        public static List<StaticIngredient> StaticIngredients = new List<StaticIngredient>()
        {
            {
                new StaticIngredient() {
                    ActionType = RecipeIngredient.ActionTypes.AddSpecificIngredient,
                    SpecificItemID = PedestalItemID,
                    IngredientSlotPosition = IngredientSlotPositions.BottomLeft,
                    IngredientID = PedestalCorelationID
                }
            },
            //needs to tie into the ingredient matcher somehow. Or, different instances of a StaticIngredient can be attached
            //to each recipe instead
            {
                new StaticIngredient() {
                    ActionType = RecipeIngredient.ActionTypes.AddGenericIngredient,
                    IngredientTypeSelector = EquipmentTagSelector,
                    IngredientSlotPosition = IngredientSlotPositions.BottomRight,
                    IngredientID = EquipmentCorelationID,
                    CountAsIngredient = true
                }
            }
        };

        public event Action<bool> EnchantingMenuEnabledChanged;
        private bool _enchantingMenuEnabled;
        public bool EnchantingMenuEnabled
        {
            get => _enchantingMenuEnabled;
            set
            {
                var oldValue = _enchantingMenuEnabled;
                _enchantingMenuEnabled = value;
                if (oldValue != _enchantingMenuEnabled)
                    EnchantingMenuEnabledChanged?.Invoke(_enchantingMenuEnabled);
            }
        }

        //public event Action AllEnchantRecipesLearnedEnabled;
        //private bool _allEnchantRecipesLearned;
        //public bool AllEnchantRecipesLearned
        //{
        //    get => _allEnchantRecipesLearned;
        //    set
        //    {
        //        _allEnchantRecipesLearned = value;
        //        if (_allEnchantRecipesLearned)
        //            AllEnchantRecipesLearnedEnabled?.Invoke();
        //    }
        //}

        public event Action<bool> ConditionalEnchantingEnabledChanged;
        private bool _conditionalEnchantingEnabled;
        public bool ConditionalEnchantingEnabled
        {
            get => _conditionalEnchantingEnabled;
            set
            {
                var prevValue = _conditionalEnchantingEnabled;
                _conditionalEnchantingEnabled = value;
                if (_conditionalEnchantingEnabled != prevValue)
                    ConditionalEnchantingEnabledChanged?.Invoke(_conditionalEnchantingEnabled);
            }
        }

        public event Action<bool> EnchantingConditionTimeChanged;
        private bool _enchantingConditionTime;
        public bool EnchantingConditionTime
        {
            get => _enchantingConditionTime;
            set
            {
                var prevValue = _enchantingConditionTime;
                _enchantingConditionTime = value;
                if (_enchantingConditionTime != prevValue)
                    EnchantingConditionTimeChanged?.Invoke(_enchantingConditionTime);
            }
        }
        public event Action<bool> EnchantingConditionWindAltarStateChanged;
        private bool _enchantingConditionWindAltarState;
        public bool EnchantingConditionWindAltarState
        {
            get => _enchantingConditionWindAltarState;
            set
            {
                var prevValue = _enchantingConditionWindAltarState;
                _enchantingConditionWindAltarState = value;
                if (_enchantingConditionWindAltarState != prevValue)
                    EnchantingConditionWindAltarStateChanged?.Invoke(_enchantingConditionWindAltarState);
            }
        }
        public event Action<bool> EnchantingConditionRegionChanged;
        private bool _enchantingConditionRegion;
        public bool EnchantingConditionRegion
        {
            get => _enchantingConditionRegion;
            set
            {
                var prevValue = _enchantingConditionRegion;
                _enchantingConditionRegion = value;
                if (_enchantingConditionRegion != prevValue)
                    EnchantingConditionRegionChanged?.Invoke(_enchantingConditionRegion);
            }
        }
        public event Action<bool> EnchantingConditionTemperatureChanged;
        private bool _enchantingConditionTemperature;
        public bool EnchantingConditionTemperature
        {
            get => _enchantingConditionTemperature;
            set
            {
                var prevValue = _enchantingConditionTemperature;
                _enchantingConditionTemperature = value;
                if (_enchantingConditionTemperature != prevValue)
                    EnchantingConditionTemperatureChanged?.Invoke(_enchantingConditionTemperature);
            }
        }
        public event Action<bool> EnchantingConditionWeatherChanged;
        private bool _enchantingConditionWeather;
        public bool EnchantingConditionWeather
        {
            get => _enchantingConditionWeather;
            set
            {
                var prevValue = _enchantingConditionWeather;
                _enchantingConditionWeather = value;
                if (_enchantingConditionWeather != prevValue)
                    EnchantingConditionWeatherChanged?.Invoke(_enchantingConditionWeather);
            }
        }
        public event Action<bool> EnchantingConditionQuestChanged;
        private bool _enchantingConditionQuest;
        public bool EnchantingConditionQuest
        {
            get => _enchantingConditionQuest;
            set
            {
                var prevValue = _enchantingConditionQuest;
                _enchantingConditionQuest = value;
                if (_enchantingConditionQuest != prevValue)
                    EnchantingConditionQuestChanged?.Invoke(_enchantingConditionQuest);
            }
        }
    }
}
