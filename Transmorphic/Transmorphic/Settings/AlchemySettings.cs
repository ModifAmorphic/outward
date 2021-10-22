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
    internal class AlchemySettings
    {
        public static readonly string PluginPath = Path.GetDirectoryName(TransmorphPlugin.Instance.Info.Location);

        public static MenuIcons AlchemyMenuIcons = new MenuIcons()
        {
            UnpressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsUnpressedAlchemy.png")) },
            HoverIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsHoverAlchemy.png")) },
            PressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsPressedAlchemy.png")) }
        };

        private static Tag _kitTag;
        public static Tag KitTag
        {
            get
            {
                if (!_kitTag.IsSet)
                    _kitTag = TagSourceManager.Instance.GetTag("45");
                return _kitTag;
            }
        }
        public static TagSourceSelector KitTagSourceSelector = new TagSourceSelector(KitTag);
        public static Guid AlchemyKitCorelationID = new Guid("11c6b4b7-17d6-4725-abc3-07ebc47ab917");
        public const int FuelItemID = 6100010; //Wood
        public static RecipeSelectorDisplayConfig RecipeSelectorDisplayConfig = new RecipeSelectorDisplayConfig()
        {
            ExtraIngredientSlotOption = ExtraIngredientSlotOptions.Both,
            StaticIngredients = new List<StaticIngredient>()
                {
                    {
                        new StaticIngredient() {
                            ActionType = RecipeIngredient.ActionTypes.AddSpecificIngredient,
                            SpecificItemID = FuelItemID,
                            IngredientSlotPosition = IngredientSlotPositions.BottomRight,
                            IngredientID = Guid.NewGuid()
                        }
                    },
                    {
                        new StaticIngredient() {
                            ActionType = RecipeIngredient.ActionTypes.AddGenericIngredient,
                            IngredientTypeSelector = KitTagSourceSelector,
                            IngredientSlotPosition = IngredientSlotPositions.BottomLeft,
                            IngredientID = AlchemyKitCorelationID
                        }
                    }
                }
        };

        public readonly static HashSet<int> AlchemyKitItemIDs = new HashSet<int>() { 5010200, 5010205 }; //Alchemy Kit , Lightweight Alchemy Kit 
        public readonly static HashSet<int> AlchemyStaticItemIDs = new HashSet<int>() { 5010200, 5010205, FuelItemID };

        public static MenuIngredientFilters MenuFilters = new MenuIngredientFilters()
        {
            AdditionalInventoryIngredientFilter = new AvailableIngredientFilter()
            {
                EnchantFilter = AvailableIngredientFilter.FilterLogic.ExcludeItems,
                SpecificItemFilter = AvailableIngredientFilter.FilterLogic.OnlyItems,
                SpecificItemIDs = AlchemyStaticItemIDs
            }
        };

        public event Action<bool> AlchemyMenuEnabledChanged;
        private bool _alchemygMenuEnabled;
        public bool AlchemyMenuEnabled
        {
            get => _alchemygMenuEnabled;
            set
            {
                var oldValue = _alchemygMenuEnabled;
                _alchemygMenuEnabled = value;
                if (oldValue != _alchemygMenuEnabled)
                    AlchemyMenuEnabledChanged?.Invoke(_alchemygMenuEnabled);
            }
        }

        public event Action<bool> DisableKitFuelReqChanged;
        private bool _disableKitFuelReq;
        public bool DisableKitFuelRequirement
        {
            get => _disableKitFuelReq;
            set
            {
                var oldValue = _disableKitFuelReq;
                _disableKitFuelReq = value;
                if (oldValue != _disableKitFuelReq)
                    DisableKitFuelReqChanged?.Invoke(_disableKitFuelReq);
            }
        }
    }
}
