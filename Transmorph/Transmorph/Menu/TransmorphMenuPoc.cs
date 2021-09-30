using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using System.Reflection;
using ModifAmorphic.Outward.Extensions;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorph
{
    public class TransmorphMenuPoc : CraftingMenu 
    {
        private IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);
        public override void Show()
        {
            Logger.LogDebug($"TransmorphMenu::Show()");
            try
            {
                base.Show();
            }
            catch (Exception ex)
            {
                Logger.LogException($"TransmorphMenu::Show() Exception.\n", ex);
            }
        }
        public void LateAwakeInit()
        {
            Logger.LogDebug($"TransmorphMenu::LateAwakeInit()");
            this.SetPrivateField<Panel, CanvasGroup>("m_canvasGroup", GetComponent<CanvasGroup>());
            CustomAwakeInit();
            //AwakeInit();
        }

        
        IngredientSelector[] m_ingredientSelectors;
        private void CustomAwakeInit()
        {
            var cmenu = (CraftingMenu)this;

            Logger.LogDebug($"CustomAwakeInit: 1");
            //m_lastFreeRecipeIngredientIDs = new int[4];
            Logger.LogDebug($"CustomAwakeInit: 1");
            cmenu.SetPrivateField("m_lastFreeRecipeIngredientIDs", new int[4]);
            Logger.LogDebug($"CustomAwakeInit: 2");
            var m_lastFreeRecipeIngredientIDs = cmenu.GetPrivateField<CraftingMenu, int[]>("m_lastFreeRecipeIngredientIDs");

            //ResetFreeRecipeLastIngredients();
            Logger.LogDebug($"CustomAwakeInit: 3");
            cmenu.InvokePrivateMethod("ResetFreeRecipeLastIngredients");
            
            Button val = null;
            var m_recipeResultDisplay = cmenu.GetPrivateField<CraftingMenu, RecipeResultDisplay>("m_recipeResultDisplay");
            if ((bool)m_recipeResultDisplay)
            {
                m_recipeResultDisplay.ParentMenu = cmenu;
                RecipeResultDisplay recipeResultDisplay = m_recipeResultDisplay;
                Logger.LogDebug($"CustomAwakeInit: 5");
                recipeResultDisplay.onSelectCallback = (UnityAction<ItemDisplay>)Delegate.Combine(recipeResultDisplay.onSelectCallback, 
                    new UnityAction<ItemDisplay>((itemDisplay) => cmenu.InvokePrivateMethod("OnResultDisplaySelected", itemDisplay)));
                val = m_recipeResultDisplay.GetComponent<Button>();
                Logger.LogDebug($"CustomAwakeInit: 6");
                UnityAction TryCraft = () => cmenu.InvokePrivateMethod("TryCraft");
                Logger.LogDebug($"CustomAwakeInit: 7");
                ((UnityEvent)(object)val.onClick).AddListener(TryCraft);
            }
            Logger.LogDebug($"CustomAwakeInit: 8");
            var m_ingredientSelectorTemplate = cmenu.GetPrivateField<CraftingMenu, IngredientSelector>("m_ingredientSelectorTemplate");
            Logger.LogDebug($"CustomAwakeInit: 9");
            if ((bool)m_ingredientSelectorTemplate)
            {
                Logger.LogDebug($"CustomAwakeInit: 10");
                cmenu.SetPrivateField("m_ingredientSelectors", new IngredientSelector[4]);
                m_ingredientSelectors = cmenu.GetPrivateField<CraftingMenu, IngredientSelector[]>("m_ingredientSelectors");
                for (int i = 0; i < m_ingredientSelectors.Length; i++)
                {
                    Logger.LogDebug($"CustomAwakeInit: 11");
                    m_ingredientSelectors[i] = UnityEngine.Object.Instantiate(m_ingredientSelectorTemplate);
                    Logger.LogDebug($"CustomAwakeInit: 12");
                    m_ingredientSelectors[i].transform.SetParent(m_ingredientSelectorTemplate.transform.parent);
                    Logger.LogDebug($"CustomAwakeInit: 13");
                    m_ingredientSelectors[i].transform.ResetLocal();
                    Logger.LogDebug($"CustomAwakeInit: 14");
                    m_ingredientSelectors[i].SetParentCraftingMenu(cmenu, i);
                    Logger.LogDebug($"CustomAwakeInit: 15");
                    int selectorIndex = i;
                    Logger.LogDebug($"CustomAwakeInit: 16");
                    m_ingredientSelectors[i].InvokePrivateMethod("AwakeInit");
                    ((UnityEvent)(object)m_ingredientSelectors[i].onClick).AddListener((UnityAction)delegate
                    {
                        OnIngredientSelectorClicked(selectorIndex);
                    });
                    Logger.LogDebug($"CustomAwakeInit: 17");
                    if (i > 0)
                    {
                        Navigation navigation = m_ingredientSelectors[i].navigation;
                        navigation.selectOnLeft = ((Selectable)(object)m_ingredientSelectors[i - 1].Button);
                        if ((bool)(UnityEngine.Object)(object)val)
                        {
                            navigation.selectOnDown = ((Selectable)(object)val);
                        }
                        m_ingredientSelectors[i].navigation = navigation;
                        navigation = m_ingredientSelectors[i - 1].navigation;
                        navigation.mode = Navigation.Mode.Explicit;
                        navigation.selectOnRight = ((Selectable)(object)m_ingredientSelectors[i].Button);
                        m_ingredientSelectors[i - 1].navigation = navigation;
                    }
                }
                var m_btnCook = cmenu.GetPrivateField<CraftingMenu, Button>("m_btnCook");
                if ((bool)(UnityEngine.Object)(object)m_btnCook)
                {
                    EventTrigger.Entry entry = null;
                    Navigation mBtnCook = m_btnCook.navigation;
                    mBtnCook.mode = Navigation.Mode.Explicit;
                    mBtnCook.selectOnUp = m_ingredientSelectors[0].Button;
                    m_btnCook.navigation = mBtnCook;
                    m_btnCook.gameObject.GetOrAddComponent<EventTrigger>().triggers.Add(entry);
                }
            }
            if ((bool)(UnityEngine.Object)(object)val)
            {
                Navigation navigation3 = ((Selectable)val).navigation;
                navigation3.mode = Navigation.Mode.Explicit;
                navigation3.selectOnUp = ((Selectable)(object)m_ingredientSelectors[0].Button);
                ((Selectable)val).navigation = navigation3;
                ((Selectable)(object)m_ingredientSelectors[0].Button).SetDownNav((Selectable)(object)val);
            }
            var m_recipeDisplayTemplate = cmenu.GetPrivateField<CraftingMenu, RecipeDisplay>("m_recipeDisplayTemplate");
            if ((bool)m_recipeDisplayTemplate)
            {
                RecipeDisplay recipeDisplay = UnityEngine.Object.Instantiate(m_recipeDisplayTemplate);
                recipeDisplay.transform.SetParent(m_recipeDisplayTemplate.transform.parent);
                recipeDisplay.transform.ResetLocal();
                recipeDisplay.Show();
                recipeDisplay.name = "_freeRecipe";
                var m_freeRecipeDisplay = cmenu.GetPrivateField<CraftingMenu, RecipeDisplay>("m_freeRecipeDisplay");
                m_freeRecipeDisplay = recipeDisplay;
                ((UnityEventBase)(object)m_freeRecipeDisplay.onClick).RemoveAllListeners();
                ((UnityEvent)(object)m_freeRecipeDisplay.onClick).AddListener((UnityAction)delegate
                {
                    OnRecipeSelected(-1);
                });
                m_freeRecipeDisplay.transform.SetAsFirstSibling();
            }
            var m_recipeSeparator = cmenu.GetPrivateField<CraftingMenu, RectTransform>("m_recipeSeparator");
            if ((bool)m_recipeSeparator)
            {
                m_recipeSeparator.name = "zSeparator";
            }
        }
        private void Awake()
        {
            Logger.LogDebug($"TransmorphMenu::Awake()");
        }
        protected override void AwakeInit()
        {
            Logger.LogDebug($"TransmorphMenu::AwakeInit()");
            try
            {
                CustomAwakeInit();
            }
            catch (Exception ex)
            {
                Logger.LogException($"TransmorphMenu::AwakeInit() Exception.\n", ex);
            }
        }
        protected override void OnHide()
        {
            Logger.LogDebug($"TransmorphMenu::OnHide()");
            try
            {
                //base.OnHide();
                Logger.LogDebug($"Panel_OnHide: Start");
                Panel_OnHide();
                Logger.LogDebug($"MenuPanel_OnHide: Start");
                MenuPanel_OnHide();
                Logger.LogDebug($"CraftingMenu_OnHide: Start");
                CraftingMenu_OnHide();
            }
            catch (Exception ex)
            {
                Logger.LogException($"TransmorphMenu::OnHide() Exception.\n", ex);
            }
        }
        public void CraftingMenu_OnHide()
        {
            Logger.LogDebug($"CraftingMenu_OnHide: 0");
            var m_tempSelectorWindow = this.GetPrivateField<CraftingMenu, ItemListSelector>("m_tempSelectorWindow");
            Logger.LogDebug($"CraftingMenu_OnHide: 1. m_tempSelectorWindow: {m_tempSelectorWindow?.name}");
            if ((bool)m_tempSelectorWindow && m_tempSelectorWindow.IsDisplayed)
            {
                Logger.LogDebug($"CraftingMenu_OnHide: 1.5");
                m_tempSelectorWindow.Hide();
            }
            Logger.LogDebug($"CraftingMenu_OnHide: 2");
            this.SetPrivateField<CraftingMenu, ItemListSelector>("m_tempSelectorWindow", null);
            //m_tempSelectorWindow = null;
            Logger.LogDebug($"CraftingMenu_OnHide: 3");
            this.SetPrivateField<CraftingMenu, CraftingStation>("m_craftingStation", null);
            
            //m_craftingStation = null;
        }
        private void MenuPanel_OnHide()
        {
            Logger.LogDebug($"MenuPanel_OnHide: 0");
            //var OnHideAction = this.GetPrivateField<MenuPanel, UnityAction>("OnHideAction");
            var onHideActionProp = typeof(MenuPanel).GetProperty("OnHideAction", BindingFlags.Instance | BindingFlags.Public);
            Logger.LogDebug($"MenuPanel_OnHide: onHideActionProp: {onHideActionProp?.Name}");
            var onHideAction = (UnityAction)onHideActionProp.GetValue((MenuPanel)this);
            Logger.LogDebug($"MenuPanel_OnHide: 1");
            if (onHideAction != null)
            {
                onHideAction();
            }
            Logger.LogDebug($"MenuPanel_OnHide: 2");
            for (int i = 0; i < NonConcurrentMenus.Length; i++)
            {
                Logger.LogDebug($"MenuPanel_OnHide: 3n");
                if ((bool)m_characterUI && m_characterUI.GetIsMenuDisplayed(NonConcurrentMenus[i]))
                {
                    m_characterUI.HideMenu(NonConcurrentMenus[i]);
                }
            }
            Logger.LogDebug($"MenuPanel_OnHide: 4");
            var m_parentPanelHolder = this.GetPrivateField<MenuPanel, MenuPanelHolder>("m_parentPanelHolder");
            Logger.LogDebug($"MenuPanel_OnHide: 5 - m_parentPanelHolder: {m_parentPanelHolder?.name}");
            if ((bool)m_parentPanelHolder)
            {
                Logger.LogDebug($"MenuPanel_OnHide: 6");
                m_parentPanelHolder.OnChildMenuHidden();
            }
        }
        private void Panel_OnHide()
        {
            var m_hideTime = this.GetPrivateField<Panel, float>("m_hideTime");
            if (m_hideTime > 0f && m_startDone)
            {
                int m_displayTarget = -1;
                this.SetPrivateField("m_displayTarget", m_displayTarget);
            }
            else
            {
                OnHideDone();
            }
        }
    }

    //public class TransmorphMenu : MenuPanel
    //{
    //    private CraftingMenu _craftingMenu;
    //    private IModifLogger Logger;
    //    public float CraftingTime;

    //    public void InitLate(CraftingMenu craftingMenu, IModifLogger logger) => (_craftingMenu, Logger) = (craftingMenu, logger);

    //    public bool IsSurvivalCrafting => _craftingMenu.IsSurvivalCrafting;
    //    public bool IsSelectingIngredient => _craftingMenu.IsSelectingIngredient;
    //    public bool IsCraftingInProgress => _craftingMenu.IsCraftingInProgress;

    //    public void IngredientSelectorHasChanged(int _selectorIndex, int _itemID) => _craftingMenu.IngredientSelectorHasChanged(_selectorIndex, _itemID);
    //    public override void OnCancelInput() => _craftingMenu.OnCancelInput();
    //    public void OnCookButtonClicked() => _craftingMenu.OnCookButtonClicked();
    //    public override void OnInfoInput() => _craftingMenu.OnInfoInput();
    //    public void OnIngredientSelectorClicked(int _selectorIndex) => _craftingMenu.OnIngredientSelectorClicked(_selectorIndex);
    //    public void OnIngredientSelectorMove(AxisEventData _data) => _craftingMenu.OnIngredientSelectorMove(_data);
    //    public void OnRecipeMove(AxisEventData _data) => _craftingMenu.OnRecipeMove(_data);
    //    public void OnRecipeSelected(int _index, bool _forceRefresh = false) => _craftingMenu.OnRecipeSelected(_index, _forceRefresh);
    //    public void RefreshItemDetailDisplay(IItemDisplay _itemDisplay) => _craftingMenu.RefreshItemDetailDisplay(_itemDisplay);
    //    public override void Show(Item _item) => _craftingMenu.Show(_item);
    //    public override void Show() => _craftingMenu.Show();

    //    public new void Hide() => _craftingMenu.Hide();

    //    //protected override void AwakeInit();
    //    protected override void OnHide()
    //    {
    //        var onhide = _craftingMenu.GetType().GetMethod(nameof(OnHide), BindingFlags.NonPublic | BindingFlags.Instance);
    //        onhide.Invoke(_craftingMenu, null);
    //    }

    //    public new void Register()
    //    {
    //        Logger.LogDebug("TransmorphMenu::Register() called.");
    //        base.Register();
    //    }
    //    //protected override void OnRefreshLanguage();
    //    //protected override void StartInit();
    //    //protected void Update();
    //}
}
