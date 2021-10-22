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
    internal class CookingSettings
    {
        public static readonly string PluginPath = Path.GetDirectoryName(TransmorphPlugin.Instance.Info.Location);

        public static MenuIcons CookingMenuIcons = new MenuIcons()
        {
            UnpressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsUnpressedCooking.png")) },
            HoverIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsHoverCooking.png")) },
            PressedIcon = new MenuIcon() { IconFilePath = Path.Combine(PluginPath, Path.Combine("assets", "tex_men_iconsPressedCooking.png")) }
        };

        private static Tag _ingredientCrafTag;
        public static Tag IngredientCrafTag
        {
            get
            {
                if (!_ingredientCrafTag.IsSet)
                    _ingredientCrafTag = TagSourceManager.Instance.GetTag("52"); //52 - Ingredient Craf
                return _ingredientCrafTag;
            }
        }
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
        public static Guid CookingKitCorelationID = new Guid("e0716380-2dfc-4cab-999d-ad2a12deb6a5");
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
                            IngredientID = CookingKitCorelationID
                        }
                    }
                }
        };

        public readonly static HashSet<int> CookingKitItemIDs = new HashSet<int>() { 5010100, 5010105 }; //Cooking Pot, Lightweight Cooking Pot
        public readonly static HashSet<int> CookingStaticItemIDs = new HashSet<int>() { 5010100, 5010105, FuelItemID };

        public static MenuIngredientFilters MenuFilters = new MenuIngredientFilters()
        {
            //BaseInventoryFilterTag = new Tag("70", "Item"),
            AdditionalInventoryIngredientFilter = new AvailableIngredientFilter()
            {
                //InventoryFilterTag = KitTag,
                EnchantFilter = AvailableIngredientFilter.FilterLogic.ExcludeItems,
                SpecificItemFilter = AvailableIngredientFilter.FilterLogic.OnlyItems,
                SpecificItemIDs = CookingStaticItemIDs
            }
        };

        public event Action<bool> CookingMenuEnabledChanged;
        private bool _cookingMenuEnabled;
        public bool CookingMenuEnabled
        {
            get => _cookingMenuEnabled;
            set
            {
                var oldValue = _cookingMenuEnabled;
                _cookingMenuEnabled = value;
                if (oldValue != _cookingMenuEnabled)
                    CookingMenuEnabledChanged?.Invoke(_cookingMenuEnabled);
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
