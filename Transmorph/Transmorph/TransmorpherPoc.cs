using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Menu;
using ModifAmorphic.Outward.Transmorph.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Transmorph
{
    internal class TransmorpherPoc
    {
        private readonly TransmorphConfigSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;
        private readonly ResourcesPrefabManager _resourcesPrefabManager;
        private readonly ItemVisualizer _itemVisualizer;

        public TransmorpherPoc(BaseUnityPlugin baseUnityPlugin, ItemVisualizer itemVisualizer, TransmorphConfigSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _itemVisualizer, _settings, _getLogger) = (baseUnityPlugin, itemVisualizer, settings, getLogger);
            // m_menus = new MenuPanel[Enum.GetNames(typeof(MenuScreens)).Length];

            CraftingMenuPatches.AwakeAfter += CraftingMenuPatches_AwakeAfter;

            CharacterUIPatches.AwakeBefore += CharacterUIPatches_AwakeBefore;

            //Menus null
            //CharacterUIPatches.StartAfter += CharacterUIPatches_StartAfter;

            //SplitScreenManagerPatches.AwakeAfter += SplitScreenManagerPatches_AwakeAfter;

            //WIP turned off to test new instantiate
            //CharacterUIPatches.ShowGameplayPanelBefore += CharacterUIPatches_ShowGameplayPanelBefore;

            //Menus null
            //SplitScreenManagerPatches.GetCachedUIAfter += SplitScreenManagerPatches_GetCachedUIAfter;

            //kind of works
            //Menus null
            //SplitPlayerPatches.InitAfter += SplitPlayerPatches_InitAfter;

            //GOing about this wrong. Take a look at adding to the CraftingMenu class instead of making a new one. New one is too much lift
            //CraftingMenuPatches.AwakeBefore += CraftingMenuPatches_AwakeBefore;
        }
        Dictionary<string, CraftingMenu> handledMenus = new Dictionary<string, CraftingMenu>();

        private void CraftingMenuPatches_AwakeAfter(CraftingMenu craftingMenu)
        {
            try
            {
                AddCustomMenu(craftingMenu);
            }
            catch (Exception ex)
            {
                Logger.LogException($"CraftingMenuPatches_AwakeAfter() Exception Adding Custom Menu.\n", ex);
            }
        }
        private void CopyFields(CraftingMenu source, CraftingMenu target)
        {
            var sourceFields = typeof(CraftingMenu).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);

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
                Logger.LogDebug($"{sourceFields[i].Name} set to '{value?.GetType()}' value: '{value}'");
            }
        }
        private void AddCustomMenu(CraftingMenu craftingMenu)
        {
            if (craftingMenu is CustomCraftingMenuPoc)
                return;

            Logger.LogDebug($"CraftingMenuPatches_AwakeAfter: Stashing instance of CraftingMenu to use as a template for later instantiations.");

            //GetParentTransform().gameObject.SetActive(true);
            //var templates = new GameObject("menu_templates");
            //templates.SetActive(false);
            //templates.transform.SetParent(GetParentTransform());

            
            var menuParentXform = craftingMenu.transform.parent;
            var isActive = menuParentXform.gameObject.activeSelf;
            menuParentXform.gameObject.SetActive(false);

            var craftMenuTemplateGo = GameObject.Instantiate(craftingMenu.gameObject, menuParentXform);
            craftMenuTemplateGo.name = "CustomCraftingMenu";
            var craftMenu = craftMenuTemplateGo.GetComponent<CraftingMenu>();
            var craftMenuTemplate = craftMenuTemplateGo.AddComponent<CustomCraftingMenuPoc>();
            CopyFields(craftMenu, craftMenuTemplate);

            UnityEngine.Object.DestroyImmediate(craftMenu);

            //activate everything to trigger the awakes.
            try
            {
                Logger.LogDebug($"Activating {craftMenuTemplateGo.name} and all its children. Current activeSelf: {craftMenuTemplateGo.activeSelf}");
                //var activeChanges = craftMenuTemplateGo.SetActiveRecursive(true);
                Logger.LogDebug($"Activating parent: {menuParentXform.gameObject.name}.");
                menuParentXform.gameObject.SetActive(true);

                //Set everything back to the way it was.
                Logger.LogDebug($"Undoing earlier activations for {craftMenuTemplateGo.name} and its children.");
                //foreach (var g in activeChanges)
                //{
                //    if (g != null)
                //        g.SetActive(!g.activeSelf);
                //}
            }
            catch (Exception ex)
            {
                Logger.LogException($"CraftingMenuPatches_AwakeAfter() activate / deactive cycle error.", ex);
            }
            //reset the parent Active status back to whatever it was before
            menuParentXform.gameObject.SetActive(isActive);

            //templates.SetActive(true);

            //recipeDisplay.SetPrivateField<RecipeDisplay, Button>("m_button", recipeXform.GetComponent<Button>());

            //find Canvas/GameplayPanels/Menus/CharacterMenus/ and associate to menu
            //var mainPanelXform = characterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel");
            //var menuPanelHolder = mainPanelXform.GetComponent<MenuPanelHolder>();
            //craftMenuTemplate.SetPrivateField("m_parentPanelHolder", menuPanelHolder);

            //craftMenuTemplate.NonConcurrentMenus = new CharacterUI.MenuScreens[0];
        }
        
        private void CharacterUIPatches_StartAfter(CharacterUI characterUI)
        {
            var menus = characterUI.GetPrivateField<CharacterUI, MenuPanel[]>("m_menus");
            var menuTypes = characterUI.GetPrivateField<CharacterUI, Type[]>("MenuTypes");
            var middlePanelXform = characterUI.transform.FindInAllChildren("MiddlePanel");
            var firstCustomScreen = Enum.GetValues(typeof(CharacterUI.MenuScreens)).Length;

            var craftingMenu = (CraftingMenu)menus.FirstOrDefault(mp => mp != null && mp is CraftingMenu);
            if (craftingMenu == default)
            {
                Logger.LogDebug("craftingMenu was null");
                return;
            }
            var active = craftingMenu.gameObject.activeSelf;
            craftingMenu.gameObject.SetActive(false);
            var inCraftMenuGo = GameObject.Instantiate(craftingMenu.gameObject, GetParentTransform());
            if (active)
                craftingMenu.gameObject.SetActive(active);
            inCraftMenuGo.name = "innerCraftMenu";

            for (int i = firstCustomScreen; i < menuTypes.Length; i++)
            {
                Logger.LogDebug($"Adding custom menu Type  {menuTypes[i]} - {i} to MiddlePannel and m_menus array.");
                var menu = (MenuPanel)middlePanelXform.gameObject.AddComponent(menuTypes[i]);
                //((TransmorphMenu)menu).InitLate(inCraftMenuGo.GetComponent<CraftingMenu>(), Logger);
                menu.Register();
                menus[i] = menu;
            }
        }

        private void CraftingMenuPatches_AwakeBefore(CraftingMenu craftingMenu)
        {
            
        }
        private void CharacterUIPatches_AwakeBefore(CharacterUIAwakeFields awakeFields)
        {
            //AddMenuTab<TransmorphMenuTab>(awakeFields.CharacterUI, "PlayerMenu_Tab_Transmorph", "btnTransmorph", awakeFields.MenuTypes.Length);

            //Array.Resize(ref awakeFields.MenuTypes, awakeFields.MenuTypes.Length + 1);
            //awakeFields.MenuTypes[awakeFields.MenuTypes.Length - 1] = typeof(TransmorphMenu);
            //awakeFields.CharacterUI.SetPrivateField("MenuTypes", awakeFields.MenuTypes);
            //Logger.LogDebug($"Resized MenuTypes to {awakeFields.MenuTypes.Length}");
            //awakeFields.CharacterUI.SetPrivateField("m_menus", new MenuPanel[awakeFields.MenuTypes.Length]);

            int menuScreenNo = awakeFields.MenuTypes.Length;

            AddMenuTab<UIMenuTab>(awakeFields.CharacterUI, "PlayerMenu_Tab_CustomCrafting", "btnCustomCrafting", menuScreenNo);

            Array.Resize(ref awakeFields.MenuTypes, awakeFields.MenuTypes.Length + 1);
            awakeFields.MenuTypes[awakeFields.MenuTypes.Length - 1] = typeof(CustomCraftingMenuPoc);
            awakeFields.CharacterUI.SetPrivateField("MenuTypes", awakeFields.MenuTypes);
            Logger.LogDebug($"Resized MenuTypes to {awakeFields.MenuTypes.Length}");
            awakeFields.CharacterUI.SetPrivateField("m_menus", new MenuPanel[awakeFields.MenuTypes.Length]);

            AddFooter(awakeFields.CharacterUI, menuScreenNo, "CustomCraftingFooter" + menuScreenNo);

        }
        private Transform _parentTransform;
        private Transform GetParentTransform()
        {
            if (_parentTransform == null)
            {
                _parentTransform = new GameObject(ModInfo.ModId.Replace(".", "_") + "_menu_prefabs").transform;
                UnityEngine.Object.DontDestroyOnLoad(_parentTransform.gameObject);
                _parentTransform.hideFlags |= HideFlags.HideAndDontSave;
                _parentTransform.gameObject.SetActive(false);
            }
            return _parentTransform;
        }
        private void AddMenuTab<T>(CharacterUI characterUI, string tabName, string buttonName, int menuId) where T : UIMenuTab
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
            newUiMenuTabGo.transform.SetSiblingIndex(craftingTabGo.transform.GetSiblingIndex() + 1);
            var newUiMenuTab = newUiMenuTabGo.AddComponent<T>();

            //Add new tab to the CharacterUI m_menuTabs array
            newUiMenuTab.LinkedMenuID = (CharacterUI.MenuScreens)menuId;
            var newMenuTab = new MenuTab()
            {
                Tab = newUiMenuTab,
                TabName = tabName
            };

            Array.Resize(ref menuTabs, menuTabs.Length + 1);
            menuTabs[menuTabs.Length - 1] = newMenuTab;
            characterUI.SetPrivateField("m_menuTabs", menuTabs);

            Logger.LogDebug($"Resized m_menuTabs to {menuTabs.Length}");

            //activate the new menu gameobject
            newUiMenuTabGo.SetActive(isActive);

        }
        private void AddFooter(CharacterUI characterUI, int menuScreen, string footerName)
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
        }
        private void CharacterUIPatches_ShowGameplayPanelBefore(CharacterUI characterUI)
        {
            var menus = characterUI.GetPrivateField<CharacterUI, MenuPanel[]>("m_menus");
            var menuTypes = characterUI.GetPrivateField<CharacterUI, Type[]>("MenuTypes");
            var middlePanelXform = characterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/MiddlePanel");
            var firstCustomScreen = Enum.GetValues(typeof(CharacterUI.MenuScreens)).Length;
            
            //foreach (var m in menus)
            //{
            //    Logger.LogTrace($"CharacterUIPatches_ShowGameplayPanelBefore:: Menu: {m?.name}, Type: {m?.GetType()}");
            //}

            var craftingMenu = (CraftingMenu)menus.FirstOrDefault(mp => mp != null && mp is CraftingMenu);
            if (craftingMenu == default)
            {
                Logger.LogDebug("craftingMenu was null");
                return;
            }


            var active = craftingMenu.gameObject.activeSelf;
            craftingMenu.gameObject.SetActive(false);
            var newMenuGo = GameObject.Instantiate(craftingMenu.gameObject, GetParentTransform());


            //UnityEngine.Object.DestroyImmediate(craftMenuTemplate.transform.Find("LeftPanel/Scroll View/Viewport/Content/Recipe").gameObject);
            //var recipeTemplate = GameObject.Instantiate(craftingMenu.transform.Find("LeftPanel/Scroll View/Viewport/Content/Recipe").gameObject, templates.transform);
            //recipeTemplate.transform.SetParent(craftMenuTemplate.transform.Find("LeftPanel/Scroll View/Viewport/Content"));
            //UnityEngine.Object.DestroyImmediate(recipeTemplate.GetComponent<RecipeDisplay>());
            //recipeTemplate.AddComponent<RecipeDisplay>();

            //Copy ingredients to new menu
            //var ingredientsXform = craftingMenu.gameObject.transform.Find("Content/Ingredients");
            //var newContentXform = newMenuGo.gameObject.transform.Find("Content");
            //var deleteme = newMenuGo.gameObject.transform.Find("Content/Ingredients");
            //UnityEngine.Object.DestroyImmediate(deleteme.gameObject);
            //var newIngredients = GameObject.Instantiate(ingredientsXform.gameObject, newContentXform);
            //newIngredients.name = ingredientsXform.name.Replace("(Clone)", "");

            Func<(RecipeResultDisplay RecipeResultDisplay, IngredientSelector IngredientSelector)> getTemplates = () =>
                (craftingMenu.GetPrivateField<CraftingMenu, RecipeResultDisplay>("m_recipeResultDisplay"), craftingMenu.GetPrivateField<CraftingMenu, IngredientSelector>("m_ingredientSelectorTemplate"));
            bool templateExists(){

                var (display, template) = getTemplates.Invoke();
                return display != null && template != null && template.onClick != null;
            };
            var setTemplateAfter = new ModifCoroutine(_getLogger);

            //var inCraftMenuGo = GameObject.Instantiate(craftingMenu.gameObject, cached.transform);
            //if (active)
                craftingMenu.gameObject.SetActive(active);
            //var ingredientSelectorTemplate = craftingMenu.GetPrivateField<CraftingMenu, IngredientSelector>("m_ingredientSelectorTemplate");

            newMenuGo.name = "Transmorphing";
            UnityEngine.Object.DestroyImmediate(newMenuGo.GetComponent<CraftingMenu>());
            newMenuGo.transform.SetParent(middlePanelXform);
            //xformMenuGo.name = "CachedTransmorphing";
            //UnityEngine.Object.DestroyImmediate(xformMenuGo.GetComponent<CraftingMenu>());
            //xformMenuGo.AddComponent<TransmorphMenu>();
            var levelRoutines = new LevelCoroutines(_baseUnityPlugin, _getLogger);
            for (int i = firstCustomScreen; i < menuTypes.Length; i++)
            {
                Logger.LogDebug($"Adding custom menu Type  {menuTypes[i]} - {i} to MiddlePannel and m_menus array.");

                //This isn't right. It's adding as a component, but really need to add the UI. Maybe add to the 
                //incraftmenuGo instead? Not sure
                //var menu = (MenuPanel)middlePanelXform.gameObject.AddComponent(menuTypes[i]);
                
                //var menu = (MenuPanel)GetParentTransform().gameObject.AddComponent(menuTypes[i]);
                //((TransmorphMenu)menu).InitLate(inCraftMenuGo.GetComponent<CraftingMenu>(), Logger);
                //levelRoutines.InvokeAfterLevelAndPlayersLoaded(NetworkLevelLoader.Instance, () => {
                //        var (recipeResultDisplay, ingredientSelector) = getTemplates.Invoke();
                //        ((CraftingMenu)menu).SetPrivateField("m_recipeResultDisplay", recipeResultDisplay);
                //        ((CraftingMenu)menu).SetPrivateField("m_ingredientSelectorTemplate", ingredientSelector);
                //        ((TransmorphMenu)menu).LateAwakeInit();
                //        newMenuGo.transform.SetParent(middlePanelXform);
                //        newMenuGo.SetActive(true);
                //        menu.Register();
                //    },
                //    604800, 1);

                _baseUnityPlugin.StartCoroutine(
                    setTemplateAfter.InvokeAfter(templateExists,
                        (t) =>
                        {
                            Logger.LogDebug($"InvokeAfter: 0");
                            var menu = (MenuPanel)newMenuGo.AddComponent(menuTypes[i]);
                            Logger.LogDebug($"InvokeAfter: 1");
                            var iselector = UnityEngine.Object.Instantiate(t.IngredientSelector);
                            iselector.InvokePrivateMethod("AwakeInit");
                            Logger.LogDebug($"InvokeAfter: 2");
                            var rdisplay = UnityEngine.Object.Instantiate(t.RecipeResultDisplay);
                            Logger.LogDebug($"InvokeAfter: 3");
                            ((CraftingMenu)menu).SetPrivateField("m_recipeResultDisplay", rdisplay);
                            Logger.LogDebug($"InvokeAfter: 4");
                            ((CraftingMenu)menu).SetPrivateField("m_ingredientSelectorTemplate", iselector);
                            Logger.LogDebug($"InvokeAfter: 5");
                            //menus[i] = menu;
                            newMenuGo.SetActive(true);
                            Logger.LogDebug($"InvokeAfter: 6");
                            characterUI.RegisterMenu(menu);
                            //menu.Register();
                            Logger.LogDebug($"InvokeAfter: 7");

                            //Both for MenuPanel_OnHide
                            ((CraftingMenu)menu).NonConcurrentMenus = new CharacterUI.MenuScreens[0];
                            var m_parentPanelHolder = craftingMenu.GetPrivateField<MenuPanel, MenuPanelHolder>("m_parentPanelHolder");
                            menu.SetPrivateField("m_parentPanelHolder", m_parentPanelHolder);
                            
                            
                            //Logger.LogDebug($"InvokeAfter: IngredientSelector.onclick null: {iselector?.onClick == null}");
                            ((TransmorphMenuPoc)menu).LateAwakeInit();
                        },
                        getTemplates, 604800, 10)
                    );
                //menus[i] = menu;
            }
        }

        private void SplitPlayerPatches_InitAfter(SplitPlayer p)
        {
            var characterUI = SplitScreenManager.Instance.GetCachedUI(p.RewiredID);

            var menus = characterUI.GetPrivateField<CharacterUI, MenuPanel[]>("m_menus");
            var menuTypes = characterUI.GetPrivateField<CharacterUI, Type[]>("MenuTypes");
            var middlePanelXform = characterUI.transform.FindInAllChildren("MiddlePanel");
            var firstCustomScreen = Enum.GetValues(typeof(CharacterUI.MenuScreens)).Length;

            foreach (var m in menus)
            {
                Logger.LogTrace($"SplitPlayerPatches_InitAfter:: Menu: {m?.name}, Type: {m?.GetType()}");
            }

            var craftingMenu = (CraftingMenu)menus.FirstOrDefault(mp => mp != null && mp is CraftingMenu);
            if (craftingMenu == default)
            {
                Logger.LogDebug("craftingMenu was null");
                return;
            }
            var active = craftingMenu.gameObject.activeSelf;
            craftingMenu.gameObject.SetActive(false);
            var inCraftMenuGo = GameObject.Instantiate(craftingMenu.gameObject, GetParentTransform());
            if (active)
                craftingMenu.gameObject.SetActive(active);
            inCraftMenuGo.name = "innerCraftMenu";

            for (int i = firstCustomScreen; i < menuTypes.Length; i++)
            {
                Logger.LogDebug($"Adding custom menu Type  {menuTypes[i]} - {i} to MiddlePannel and m_menus array.");
                var menu = (MenuPanel)middlePanelXform.gameObject.AddComponent(menuTypes[i]);
                //((TransmorphMenu)menu).InitLate(inCraftMenuGo.GetComponent<CraftingMenu>(), Logger);
                menu.Register();
                menus[i] = menu;
            }
        }

        


        private void SplitScreenManagerPatches_GetCachedUIAfter((SplitScreenManager SplitScreenManager, int Id, SplitScreenManagerPatches.CacheStatus CacheStatus, CharacterUI ResultRef) obj)
        {
            var menus = obj.ResultRef.GetPrivateField<CharacterUI, MenuPanel[]>("m_menus");
            if (menus != null)
            {
                foreach (var m in menus)
                {
                    Logger.LogTrace($"Menu: {m?.name}, Type: {m?.GetType()}");
                }
                //if (!menus.Any(m => m is TransmorphMenu))
                //    Array.Resize(ref menus, menus.Length + 1);
                //var transMenu = obj.ResultRef.gameObject.AddComponent<TransmorphMenu>();

                //var craftingMenu = (CraftingMenu)menus.FirstOrDefault(p => p != null && p is CraftingMenu);
                ////transMenu.InitLate(craftingMenu);
                //menus[menus.Length + 1] = transMenu;
                //obj.ResultRef.SetPrivateField("m_menus", menus);
                //Logger.LogDebug($"Resized m_menus to {menus.Length}");
            }
        }

        private void SplitScreenManagerPatches_AwakeAfter(SplitScreenManager splitScreenManager, CharacterUI characterUI)
        {
            var menuTabs = characterUI.GetPrivateField<CharacterUI, MenuTab[]>("m_menuTabs");
            Array.Resize(ref menuTabs, menuTabs.Length + 1);
            menuTabs[menuTabs.Length - 1] = new MenuTab()
            {
                Tab = menuTabs.First(t => t.TabName == "PlayerMenu_Tab_Crafting").Tab,
                TabName = "PlayerMenu_Tab_Transmorph"
            };
            characterUI.SetPrivateField("m_menuTabs", menuTabs);
            Logger.LogDebug($"Resized m_menuTabs to {menuTabs.Length}");


            var menuTypes = characterUI.GetPrivateField<CharacterUI, Type[]>("MenuTypes");
            Array.Resize(ref menuTypes, menuTypes.Length + 1);
            menuTypes[menuTypes.Length - 1] = typeof(TransmorphMenuPoc);
            characterUI.SetPrivateField("MenuTypes", menuTypes);
            Logger.LogDebug($"Resized MenuTypes to {menuTypes.Length}");

        }
        public void SetTransmorph(Transmorph transmorph)
        {
            _itemVisualizer.RegisterItemVisual(transmorph.SourceItemID, transmorph.ItemUID);

            var sourceItem = _resourcesPrefabManager.GetItemPrefab(transmorph.SourceItemID);
        }
        public void SetTransmorph(int itemID, string targetUID)
        {
            _itemVisualizer.RegisterItemVisual(itemID, targetUID);
        }

        public void SaveTransmorph(Item sourceItem, Item targetItem)
        {
            var xMorph = new Transmorph() 
            { 
                ItemUID = targetItem.UID,
                SourceItemID = sourceItem.ItemID
            };
        }
        internal class Transmorph
        {
            public string ItemUID { get; set; }
            public int SourceItemID { get; set; }
        }
    }
}
