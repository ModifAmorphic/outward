using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Localization;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Crafting.Services
{
    internal class CraftingMenuService
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();


        public CraftingMenuService(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);

        public GameObject AddMenuTab(CharacterUI characterUI, string tabName, string tabDisplayName, string buttonName, int menuId, int orderNo)
        {
            var menuTabs = characterUI.GetPrivateField<CharacterUI, MenuTab[]>("m_menuTabs");
            //var section = characterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/TopPanel/Sections");
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
            //newUiMenuTabGo.transform.parent = section;
            newUiMenuTabGo.transform.SetSiblingIndex(craftingTabGo.transform.GetSiblingIndex() + 1 + orderNo);
            var newUiMenuTab = newUiMenuTabGo.AddComponent<UIMenuTab>();

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
            
            Logger.LogDebug($"{nameof(CraftingMenuService)}::{nameof(AddMenuTab)}: Resized m_menuTabs to {menuTabs.Length}");
            
            //activate the new menu gameobject
            newUiMenuTabGo.SetActive(isActive);

            return newUiMenuTabGo;
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
            //GetParentTransform().gameObject.SetActive(true);
            //var templates = new GameObject("menu_templates");
            //templates.SetActive(false);
            //templates.transform.SetParent(GetParentTransform());

            var menuParentXform = baseCraftingMenu.transform.parent;
            var isActive = menuParentXform.gameObject.activeSelf;
            menuParentXform.gameObject.SetActive(false);

            var craftMenuTemplateGo = GameObject.Instantiate(baseCraftingMenu.gameObject, menuParentXform);
            craftMenuTemplateGo.name = menuMeta.MenuName;
            var craftMenu = craftMenuTemplateGo.GetComponent<CraftingMenu>();
            var craftMenuTemplate = (CraftingMenu)craftMenuTemplateGo.AddComponent(menuMeta.MenuType);
            CopyFields(craftMenu, craftMenuTemplate);

            UnityEngine.Object.DestroyImmediate(craftMenu);
            
            //remove clones that would otherwise be duplicated when the menu Awakes.
            RemoveMenuClones(craftMenuTemplateGo);

            //replace the Built in RecipeResultDisplay with the custom one.
            ReplaceRecipeResultDisplay(craftMenuTemplateGo);
            //activate everything to trigger the awakes.
            try
            {
                
                //Logger.LogDebug($"Activating {craftMenuTemplateGo.name} and all its children. Current activeSelf: {craftMenuTemplateGo.activeSelf}");
                //var activeChanges = craftMenuTemplateGo.SetActiveRecursive(true);
                Logger.LogDebug($"{nameof(CraftingMenuService)}::{nameof(AddCustomMenu)}: Activating menu parent: {menuParentXform.gameObject.name}.");
                menuParentXform.gameObject.SetActive(true);

                //Set everything back to the way it was.
                //Logger.LogDebug($"Undoing earlier activations for {craftMenuTemplateGo.name} and its children.");
                //foreach (var g in activeChanges)
                //{
                //    if (g != null)
                //        g.SetActive(!g.activeSelf);
                //}
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(CraftingMenuService)}::{nameof(AddCustomMenu)}: CraftingMenuPatches_AwakeAfter() activate / deactive cycle error.", ex);
            }
            //reset the parent Active status back to whatever it was before
            menuParentXform.gameObject.SetActive(isActive);

            return craftMenuTemplateGo;
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
            
            Logger.LogDebug($"{nameof(CraftingMenuService)}::{nameof(AddIngredientTags)}: Expanded m_craftingStationIngredientTags by {expandBy}.");

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
                Logger.LogTrace($"{sourceFields[i].Name} set to '{value?.GetType()}' value: '{value}'");
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
