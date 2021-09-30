using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Transmorph.Menu
{
    public class CustomCraftingMenuPoc : CraftingMenu
    {
        private IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

		//private void Awake()
		//{
		//	Logger.LogDebug($"CustomCraftingMenu::Awake()");

		//	var go = this.gameObject;
		//	//Attach recipedisplay button
		//	var recipeXform = go.transform.Find("LeftPanel/Scroll View/Viewport/Content/Recipe");
		//	var recipeDisplay = recipeXform.gameObject.GetComponent<RecipeDisplay>();
		//	Logger.LogDebug($"Setting private field m_recipeDisplayTemplate of {this.name} to found RecipeDisplay component '{recipeDisplay?.name}'");
		//	this.SetPrivateField<CraftingMenu, RecipeDisplay>("m_recipeDisplayTemplate", recipeDisplay);

		//	//Attach IngredientSelector template
		//	var ingredientSelectorXform = go.transform.Find("Content/Ingredients/IngredientSelector");
		//	this.SetPrivateField<CraftingMenu, IngredientSelector>("m_ingredientSelectorTemplate", ingredientSelectorXform.gameObject.GetComponent<IngredientSelector>());

		//	var itemDisplayGridXform = go.transform.Find("Content/CraftingResult/ItemDisplayGrid");
		//	this.SetPrivateField<CraftingMenu, RecipeResultDisplay>("m_recipeResultDisplay", itemDisplayGridXform.gameObject.GetComponent<RecipeResultDisplay>());
		//	this.NonConcurrentMenus = new CharacterUI.MenuScreens[0];
		//}
		protected override void AwakeInit()
		{
			Logger.LogDebug($"CustomCraftingMenu::AwakeInit()");
			try
			{
				//return;
				////Show requirements
				//var recipeXform = this.gameObject.transform.Find("LeftPanel/Scroll View/Viewport/Content/Recipe");
    //            var recipeDisplay = recipeXform.gameObject.GetComponent<RecipeDisplay>();
    //            this.SetPrivateField<CraftingMenu, RecipeDisplay>("m_recipeDisplayTemplate", recipeXform.gameObject.GetComponent<RecipeDisplay>());
				//var freeRecipeDescriptionXform = this.gameObject.transform.Find("RightPanel/FreeRecipeDescription");
				//this.SetPrivateField<CraftingMenu, GameObject>("m_freeRecipeDescriptionPanel", freeRecipeDescriptionXform.gameObject);
				////end show

				////Attach IngredientSelector template
				//var ingredientSelectorXform = this.gameObject.transform.Find("Content/Ingredients/IngredientSelector");
				//this.SetPrivateField<CraftingMenu, IngredientSelector>("m_ingredientSelectorTemplate", ingredientSelectorXform.gameObject.GetComponent<IngredientSelector>());

				//Remove these because they will be readded in awakeinit
				var clonedSelector = this.gameObject.transform.Find("Content/Ingredients/IngredientSelector(Clone)");
				while (clonedSelector?.gameObject != null)
                {
					Logger.LogDebug($"CustomCraftingMenu::AwakeInit(): Destroying clonedSelector {clonedSelector?.gameObject?.name}");
					UnityEngine.Object.DestroyImmediate(clonedSelector.gameObject);
					clonedSelector = this.gameObject.transform.Find("Content/Ingredients/IngredientSelector(Clone)");
				}

				//var itemDisplayGridXform = this.gameObject.transform.Find("Content/CraftingResult/ItemDisplayGrid");
				//this.SetPrivateField<CraftingMenu, RecipeResultDisplay>("m_recipeResultDisplay", itemDisplayGridXform.gameObject.GetComponent<RecipeResultDisplay>());

				//this.NonConcurrentMenus = new CharacterUI.MenuScreens[0];
				base.AwakeInit();
			}
			catch (Exception ex)
			{
				Logger.LogException($"CustomCraftingMenu::AwakeInit() Exception.\n", ex);
			}
		}

		public override void Show()
        {
            Logger.LogDebug($"CustomCraftingMenu::Show()");
            try
            {
				//This can be uncommented. It works, but easier to troubleshoot with methods after.
				//base.Show();
				//return;
				this.SetPrivateField<Panel, bool>("m_showWanted", true);
				Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking UIElement_Show()");
				UIElement_Show();
				Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking Panel_Show()");
				Panel_Show();
				Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking MenuPanel_Show()");
				MenuPanel_Show();
				Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking CraftingMenu_Show()");
				CraftingMenu_Show();

			}
            catch (Exception ex)
            {
                Logger.LogException($"CustomCraftingMenu::Show() Exception.\n", ex);
            }
        }
        private void UIElement_Show()
        {
			var m_starting = this.GetPrivateField<UIElement, bool>("m_starting");
			if (!m_startDone && !m_starting)
			{
				Start();
			}
			if ((bool)PanelContent && !PanelContent.activeSelf)
			{
				PanelContent.SetActive(value: true);
			}
			//m_hideWanted = false;
			this.SetPrivateField<UIElement, bool>("m_hideWanted", false);
			
			m_lastToggleTime = Time.time;
		}
		private void Panel_Show()
        {
			//m_showWanted = true;
			//base.Show();
			var m_showTime = this.GetPrivateField<Panel, float>("m_showTime");
			var m_canvasGroup = this.GetPrivateField<Panel, CanvasGroup>("m_canvasGroup");
			if (m_showTime > 0f)
			{
				if ((bool)(UnityEngine.Object)(object)m_canvasGroup)
				{
					m_canvasGroup.interactable = true;
				}
				//m_displayTarget = 1;
				this.SetPrivateField<Panel, int>("m_displayTarget", 1);
			}
			else if ((bool)(UnityEngine.Object)(object)m_canvasGroup)
			{
				m_canvasGroup.alpha = 1f;
				m_canvasGroup.interactable = true;
			}
			if (AutoPlayShowSound)
			{
				PlayShowSound();
			}
			//OnShowInvokeFocus();
			this.InvokePrivateMethod<Panel>("OnShowInvokeFocus");
		}
		private void MenuPanel_Show()
        {
			var m_parentPanelHolder = this.GetPrivateField<MenuPanel, MenuPanelHolder>("m_parentPanelHolder");
			if ((bool)m_parentPanelHolder)
			{
				m_parentPanelHolder.OnChildMenuShown();
			}
		}
        private void CraftingMenu_Show()
        {
			Logger.LogDebug($"CraftingMenu_Show: 0");
			var m_craftingStation = this.GetPrivateField<CraftingMenu, CraftingStation>("m_craftingStation");
			var m_lblStationName = this.GetPrivateField<CraftingMenu, Text>("m_lblStationName");
			var m_simpleMode = false;

			Logger.LogDebug($"CraftingMenu_Show: 1");
			if (m_craftingStation == null)
			{
				//m_craftingStationType = Recipe.CraftingType.Survival;
				this.SetPrivateField<CraftingMenu, Recipe.CraftingType>("m_craftingStationType", Recipe.CraftingType.Survival);
				Logger.LogDebug($"CraftingMenu_Show: 1:A:1");
				if ((bool)(UnityEngine.Object)(object)m_lblStationName)
				{
					m_lblStationName.text = "";
				}
				m_simpleMode = false;
				this.SetPrivateField<CraftingMenu, bool>("m_simpleMode", m_simpleMode);
				Logger.LogDebug($"CraftingMenu_Show: 1:A:2");
			}
			else
			{
				Logger.LogDebug($"CraftingMenu_Show: 1:B:1");
				if ((bool)(UnityEngine.Object)(object)m_lblStationName)
				{
					m_lblStationName.text = m_craftingStation.DisplayName;
				}
				m_simpleMode = !m_craftingStation.AllowComplexRecipe;
				this.SetPrivateField<CraftingMenu, bool>("m_simpleMode", m_simpleMode);
				Logger.LogDebug($"CraftingMenu_Show: 1:B:2");
			}
			Logger.LogDebug($"CraftingMenu_Show: 2");
			var m_singleIngredientBackground = this.GetPrivateField<CraftingMenu, Image>("m_singleIngredientBackground");
			if ((bool)(UnityEngine.Object)(object)m_singleIngredientBackground)
			{
				((Graphic)(object)m_singleIngredientBackground).SetAlpha(m_simpleMode ? 1 : 0);
			}
			Logger.LogDebug($"CraftingMenu_Show: 3");
			var m_multipleIngrenentsBrackground = this.GetPrivateField<CraftingMenu, Image>("m_multipleIngrenentsBrackground");
			if ((bool)(UnityEngine.Object)(object)m_multipleIngrenentsBrackground)
			{
				((Graphic)(object)m_multipleIngrenentsBrackground).SetAlpha((!m_simpleMode) ? 1 : 0);
			}
			Logger.LogDebug($"CraftingMenu_Show: 4");
			var m_ingredientSelectors = this.GetPrivateField<CraftingMenu, IngredientSelector[]>("m_ingredientSelectors");
			for (int i = 1; i < m_ingredientSelectors.Length; i++)
			{
				m_ingredientSelectors[i].Show(!m_simpleMode);
			}
			Logger.LogDebug($"CraftingMenu_Show: 5");
			int num = -1;
			Sprite overrideSprite = null;
			var m_craftingStationType = this.GetPrivateField<CraftingMenu, Recipe.CraftingType>("m_craftingStationType");
			var m_survivalCraftingBg = this.GetPrivateField<CraftingMenu, Sprite>("m_survivalCraftingBg");
			var m_cookingPotCraftingBg = this.GetPrivateField<CraftingMenu, Sprite>("m_cookingPotCraftingBg");
			var m_cookingFireCraftingBg = this.GetPrivateField<CraftingMenu, Sprite>("m_cookingFireCraftingBg");
			var m_alchemyCraftingBg = this.GetPrivateField<CraftingMenu, Sprite>("m_alchemyCraftingBg");
			Logger.LogDebug($"CraftingMenu_Show: 6");
			switch (m_craftingStationType)
			{
				case Recipe.CraftingType.Survival:
					num = 1;
					overrideSprite = m_survivalCraftingBg;
					break;
				case Recipe.CraftingType.Cooking:
					if (m_craftingStation.AllowComplexRecipe)
					{
						num = 3;
						overrideSprite = m_cookingPotCraftingBg;
					}
					else
					{
						num = 2;
						overrideSprite = m_cookingFireCraftingBg;
					}
					break;
				case Recipe.CraftingType.Alchemy:
					num = 0;
					overrideSprite = m_alchemyCraftingBg;
					break;
				case Recipe.CraftingType.Forge:
					num = 4;
					overrideSprite = m_alchemyCraftingBg;
					break;
			}
			Logger.LogDebug($"CraftingMenu_Show: 7");
			var m_lblFreeRecipeDescription = this.GetPrivateField<CraftingMenu, Text>("m_lblFreeRecipeDescription");
			var m_freeRecipesLocKey = typeof(CraftingMenu).GetField("m_freeRecipesLocKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
				.GetValue(null) as string[];
			

			var m_imgCraftingBackground = this.GetPrivateField<CraftingMenu, Image>("m_imgCraftingBackground");
			Logger.LogDebug($"CraftingMenu_Show: 8");
			if ((bool)(UnityEngine.Object)(object)m_lblFreeRecipeDescription)
			{
				m_lblFreeRecipeDescription.text = LocalizationManager.Instance.GetLoc(m_freeRecipesLocKey[num]);
			}
			Logger.LogDebug($"CraftingMenu_Show: 9");
			if ((bool)(UnityEngine.Object)(object)m_imgCraftingBackground)
			{
				m_imgCraftingBackground.overrideSprite = overrideSprite;
			}
			Logger.LogDebug($"CraftingMenu_Show: 10");
			//ResetFreeRecipeLastIngredients();
			this.InvokePrivateMethod<CraftingMenu>("ResetFreeRecipeLastIngredients");

			Logger.LogDebug($"CraftingMenu_Show: 11");
			//m_allRecipes = RecipeManager.Instance.GetRecipes(m_craftingStationType, base.LocalCharacter);
			this.SetPrivateField<CraftingMenu, List<Recipe>>("m_allRecipes", RecipeManager.Instance.GetRecipes(m_craftingStationType, base.LocalCharacter));

			Logger.LogDebug($"CraftingMenu_Show: 12");
			//m_refreshComplexeRecipeRequired = true;
			this.SetPrivateField<CraftingMenu, bool>("m_refreshComplexeRecipeRequired", true);

			Logger.LogDebug($"CraftingMenu_Show: 13");
			//RefreshAutoRecipe();
			this.InvokePrivateMethod<CraftingMenu>("RefreshAutoRecipe");

			Logger.LogDebug($"CraftingMenu_Show: 14");
			OnRecipeSelected(-1, _forceRefresh: true);
		}
        protected override void OnHide()
        {
            Logger.LogDebug($"CustomCraftingMenu::OnHide()");
            try
            {
                base.OnHide();
            }
            catch (Exception ex)
            {
                Logger.LogException($"CustomCraftingMenu::OnHide() Exception.\n", ex);
            }
        }
    }
}
