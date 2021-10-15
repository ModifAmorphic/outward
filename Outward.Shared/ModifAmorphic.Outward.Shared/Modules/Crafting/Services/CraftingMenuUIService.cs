using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Localization;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Modules.Crafting.Services
{
    internal class CraftingMenuUIService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();


        public CraftingMenuUIService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public GameObject AddMenuTab(CharacterUI characterUI, string tabName, string tabDisplayName, string buttonName, int menuId, string orderAfterBtn, MenuIcons menuIcons)
        {
            var menuTabs = characterUI.GetPrivateField<CharacterUI, MenuTab[]>("m_menuTabs");
            

            //Get the existing crafting tab and clone it
            var craftingTabGo = menuTabs.First(t => t.TabName == "PlayerMenu_Tab_Crafting").Tab.gameObject;
            var isActive = craftingTabGo.activeSelf;
            craftingTabGo.SetActive(false);

            var newUiMenuTabGo = UnityEngine.Object.Instantiate(craftingTabGo, craftingTabGo.transform.parent);
            if (isActive != false)
                craftingTabGo.SetActive(isActive);
            //Destroy the UIMenuTab and replace it
            UnityEngine.Object.DestroyImmediate(newUiMenuTabGo.GetComponent<UIMenuTab>());
            newUiMenuTabGo.name = buttonName;

            var newUiMenuTab = newUiMenuTabGo.AddComponent<UIMenuTab>();
            newUiMenuTab.transform.SetAsLastSibling();
            //Add new tab to the CharacterUI m_menuTabs array
            newUiMenuTab.LinkedMenuID = (CharacterUI.MenuScreens)menuId;
            var newMenuTab = new MenuTab()
            {
                Tab = newUiMenuTab,
                TabName = tabName
            };
            LocalizationService.RegisterLocalization(tabName, tabDisplayName);

            Array.Resize(ref menuTabs, menuTabs.Length + 1);
            menuTabs[menuTabs.Length - 1] = newMenuTab;
            characterUI.SetPrivateField("m_menuTabs", menuTabs);
            
            Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(AddMenuTab)}: Resized m_menuTabs to {menuTabs.Length}");

            //activate the new menu gameobject
            newUiMenuTabGo.SetActive(isActive);

            SetTabOrder(buttonName, orderAfterBtn, characterUI);

            if (menuIcons != null)
                SetMenuIcons(newUiMenuTabGo, menuIcons);

            return newUiMenuTabGo;
        }
        public void EnableMenuTab(GameObject menuTab)
        {
            if (!menuTab.activeSelf)
                menuTab.SetActive(true);
        }
        public void DisableMenuTab(GameObject menuTab)
        {
            if (menuTab.activeSelf)
                menuTab.SetActive(false);
        }
        private void SetMenuIcons(GameObject menuBtn, MenuIcons menuIcons)
        {
            var toggle = menuBtn.GetComponent<Toggle>();

            
            var spriteState = new SpriteState()
            { 
                pressedSprite = toggle.spriteState.pressedSprite.Clone(menuIcons.PressedIcon.IconFilePath, menuIcons.PressedIcon.SpriteName, menuIcons.PressedIcon.TextureName),
                highlightedSprite = toggle.spriteState.pressedSprite.Clone(menuIcons.HoverIcon.IconFilePath, menuIcons.HoverIcon.SpriteName, menuIcons.HoverIcon.TextureName)
            };

            toggle.spriteState = spriteState;

            Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(SetMenuIcons)}: Replaced HighlightedSprite with new Sprite '{menuIcons.HoverIcon.SpriteName}'. PressedSprite is now {toggle.spriteState.highlightedSprite}. File: {menuIcons.HoverIcon.IconFilePath}.");

            var unpressed = menuBtn.GetComponent<Image>();
            unpressed.ReplaceSpriteIcon(menuIcons.UnpressedIcon.IconFilePath, menuIcons.UnpressedIcon.SpriteName, menuIcons.UnpressedIcon.TextureName);
            Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(SetMenuIcons)}: Replaced UnpressedSprite with new Sprite '{menuIcons.UnpressedIcon.SpriteName}', File: {menuIcons.UnpressedIcon.IconFilePath}.");

            var pressedGo = menuBtn.transform.Find("Pressed").gameObject;
            var pressed = pressedGo.GetComponent<Image>();
            pressed.ReplaceSpriteIcon(menuIcons.PressedIcon.IconFilePath, menuIcons.PressedIcon.SpriteName, menuIcons.PressedIcon.TextureName);
            Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(SetMenuIcons)}: Replaced PressedSprite with new Sprite '{menuIcons.PressedIcon.SpriteName}', File: {menuIcons.PressedIcon.IconFilePath}.");
        }
        private void SetTabOrder(string targetBtn, string orderAfterBtn, CharacterUI characterUI)
        {
            var section = characterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/TopPanel/Sections");
            var targetTab = section.GetComponentsInChildren<UIMenuTab>()
                                        .First(m => m.name == targetBtn);
            var orderedTabs = section.GetComponentsInChildren<UIMenuTab>()
                                        .Where(m => m.name != targetBtn)
                                        .OrderBy(m => m.transform.GetSiblingIndex())
                                        .ToArray();
            int endIndex = orderedTabs.Last().transform.GetSiblingIndex();

            int shiftIndex = 1;
            int iteration = 0;
            for (int i = orderedTabs.Length - 1; i >= 0; i--)
            {
                if (orderedTabs[i].name == orderAfterBtn)
                {
                    targetTab.transform.SetSiblingIndex(endIndex + shiftIndex - iteration);
                    Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(SetTabOrder)}: {targetTab.name}: SetSiblingIndex({(endIndex + shiftIndex - iteration)}). GetSiblingIndex(): {targetTab.transform.GetSiblingIndex()}");
                    shiftIndex = 0;
                }

                orderedTabs[i].transform.SetSiblingIndex(endIndex + shiftIndex - iteration);
                Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(SetTabOrder)}: {orderedTabs[i].name}: {orderedTabs[i].transform.GetSiblingIndex()}");

                iteration++;
            }
        }
        private void DisableTitleLabel(GameObject menu)
        {
            const string lblTitlePath = "Content/lblTitle";
            var lblTitleXform = menu.transform.Find(lblTitlePath);
            if (lblTitleXform != null)
            {
                lblTitleXform.gameObject.SetActive(false);
                Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(DisableTitleLabel)}: Disabled title label {lblTitleXform.gameObject.name} under menu {menu.name}.");
            }
            else
                Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(DisableTitleLabel)}: Could not find title under menu {menu.name}. Tried to find: {lblTitlePath}.");
        }
        public GameObject AddFooter(CharacterUI characterUI, int menuScreen, string footerName)
        {
            var craftFooter = characterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/BottomPanel/Crafting PanelFooter").gameObject;
            var isActive = craftFooter.activeSelf;
            craftFooter.SetActive(false);
            var customCraftFooter = UnityEngine.Object.Instantiate(craftFooter, craftFooter.transform.parent);
            UnityEngine.Object.DestroyImmediate(customCraftFooter.GetComponent<CookingFooterPanel>());
            craftFooter.SetActive(isActive);

            var footerPanel = customCraftFooter.gameObject.AddComponent<CookingFooterPanel>();
            footerPanel.LinkedMenuID = (CharacterUI.MenuScreens)menuScreen;
            footerPanel.Footer = customCraftFooter.GetComponentInChildren<FooterButtonHolder>(true);
            footerPanel.name = footerName;
            customCraftFooter.SetActive(isActive);

            return customCraftFooter;
        }
        public GameObject AddCustomMenu(CraftingMenu baseCraftingMenu, CraftingMenuMetadata menuMeta)
        {
            var menuParentXform = baseCraftingMenu.transform.parent;
            var isActive = menuParentXform.gameObject.activeSelf;
            menuParentXform.gameObject.SetActive(false);

            var newMenuGo = GameObject.Instantiate(baseCraftingMenu.gameObject, menuParentXform);
            newMenuGo.name = menuMeta.MenuName;
            var craftMenu = newMenuGo.GetComponent<CraftingMenu>();
            var newMenu = (CraftingMenu)newMenuGo.AddComponent(menuMeta.MenuType);
            CopyFields(craftMenu, newMenu);

            UnityEngine.Object.DestroyImmediate(craftMenu);
            
            //remove clones that would otherwise be duplicated when the menu Awakes.
            RemoveMenuClones(newMenuGo);

            //replace the Built in RecipeResultDisplay with the custom one.
            ReplaceRecipeResultDisplay(newMenuGo);
            //activate everything to trigger the awakes.
            try
            {
                Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(AddCustomMenu)}: Activating menu parent: {menuParentXform.gameObject.name}.");
                menuParentXform.gameObject.SetActive(true);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuUIService)}::{nameof(AddCustomMenu)}: CraftingMenuPatches_AwakeAfter() activate / deactive cycle error.", ex);
            }
            //reset the parent Active status back to whatever it was before
            menuParentXform.gameObject.SetActive(isActive);
            
            //Disable title label for Alchemy and Cooking menus, otherwise they display "Pocket".
            if (newMenu is CustomCraftingMenu customMenu)
                if (customMenu.PermanentCraftingStationType == Recipe.CraftingType.Alchemy
                    || customMenu.PermanentCraftingStationType == Recipe.CraftingType.Cooking)
                        DisableTitleLabel(newMenuGo);
            
            return newMenuGo;
        }
        public List<int> AddIngredientTags(int tags)
        {
            var m_craftingStationIngredientTags = TagSourceManager.Instance.GetPrivateField<TagSourceManager, TagSourceSelector[]>("m_craftingStationIngredientTags");
            var defaultTag = m_craftingStationIngredientTags[2];
            var baseStations = Enum.GetValues(typeof(Recipe.CraftingType)).Length;

            //There's a "Count" CraftingType that doesn't get added by the base game. The m_craftingStationIngredientTags
            //array needs to be expanded to include a placehold for it so any new custom crafting types align to the correct index value.

            var padding = m_craftingStationIngredientTags.Length < baseStations ? baseStations - m_craftingStationIngredientTags.Length : 0;
            var expandBy = padding + tags;
            var newStartIndex = m_craftingStationIngredientTags.Length + padding;

            Array.Resize(ref m_craftingStationIngredientTags, m_craftingStationIngredientTags.Length + expandBy);

            var addedTagIds = new List<int>();

            for (int i = newStartIndex; i < m_craftingStationIngredientTags.Length; i++)
            {
                m_craftingStationIngredientTags[i] = defaultTag;
                addedTagIds.Add(i);
            }

            TagSourceManager.Instance.SetPrivateField("m_craftingStationIngredientTags", m_craftingStationIngredientTags);
            
            Logger.LogDebug($"{nameof(CraftingMenuUIService)}::{nameof(AddIngredientTags)}: Expanded m_craftingStationIngredientTags by {expandBy}.");

            return addedTagIds;

        }
        public int AddIngredientTag()
        {
            return AddIngredientTags(1)[0];
        }
        private void CopyFields<T>(T source, T target)
        {
            var sourceFields = typeof(T).GetFields(System.Reflection.BindingFlags.Public 
                | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance 
                | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);

            for (int i = 0; i < sourceFields.Length; i++)
            {
                object value;
                if (sourceFields[i].IsStatic)
                {
                    value = sourceFields[i].GetValue(null);
                    sourceFields[i].SetValue(source.GetType(), value, System.Reflection.BindingFlags.FlattenHierarchy, null, null);
                }
                else
                {
                    value = sourceFields[i].GetValue(source);
                    sourceFields[i].SetValue(target, value, System.Reflection.BindingFlags.FlattenHierarchy, null, null);
                }
                //Logger.LogTrace($"{sourceFields[i].Name} set to '{value?.GetType()}' value: '{value}'");
            }
        }
        private void RemoveMenuClones(GameObject menu)
        {
            const string clonedSelectorPath = "Content/Ingredients/IngredientSelector(Clone)";
            const string clonedFreeRecipePath = "LeftPanel/Scroll View/Viewport/Content/_freeRecipe";

            //Remove these because they will be readded in awakeinit
            var clonedSelector = menu.transform.Find(clonedSelectorPath);
            while (clonedSelector?.gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(clonedSelector.gameObject);
                clonedSelector = menu.transform.Find(clonedSelectorPath);
            }
            var clonedFreeRecipe = menu.transform.Find(clonedFreeRecipePath);
            while (clonedFreeRecipe?.gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(clonedFreeRecipe.gameObject);
                clonedFreeRecipe = menu.transform.Find(clonedFreeRecipePath);
            }
        }
        private void ReplaceRecipeResultDisplay(GameObject menu)
        {
            const string recipeDisplayPath = "Content/CraftingResult/ItemDisplayGrid";
            var displayGrid = menu.transform.Find(recipeDisplayPath).gameObject;

            var existingDisplay = displayGrid.GetComponent<RecipeResultDisplay>();
            var customDisplay = displayGrid.AddComponent<CustomRecipeResultDisplay>();
            customDisplay.SetLoggerFactory(_loggerFactory);
            CopyFields(existingDisplay, customDisplay);
            UnityEngine.Object.Destroy(existingDisplay);

            var craftingMenu = menu.GetComponent<CustomCraftingMenu>();
            craftingMenu.SetPrivateField<CraftingMenu, RecipeResultDisplay>("m_recipeResultDisplay", customDisplay);
        }
    }
}
