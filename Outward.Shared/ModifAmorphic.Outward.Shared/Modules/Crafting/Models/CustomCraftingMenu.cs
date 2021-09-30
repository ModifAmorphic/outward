using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CustomCraftingMenu : CraftingMenu
    {
		private static IModifLogger nullLogger = new NullLogger();

		protected IModifLogger Logger => LoggerFactory.Invoke();
		private Func<IModifLogger> _loggerFactory = () => nullLogger;
		protected Func<IModifLogger> LoggerFactory
		{
			get => _loggerFactory;
			set => _loggerFactory = value;
		}
		/// <summary>
		/// Skips over the <see cref="CraftingMenu.AwakeInit"/> method in the dependency chain, invoking <see cref="Panel.AwakeInit"/> next instead.
		/// </summary>
		protected bool BypassCraftingMenuAwakeInit = false;
		/// <summary>
		/// Skips over the <see cref="CraftingMenu.Show"/> method in the dependency chain, invoking <see cref="MenuPanel.Show"/> next instead.
		/// </summary>
		//protected bool BypassCraftingMenuShow = false;

		Recipe.CraftingType? _permenantCraftingStationType;
		public Recipe.CraftingType? PermanentCraftingStationType
		{
			get => _permenantCraftingStationType;
			protected set
			{
				if (value != null && !value.Value.IsDefinedValue())
					throw new ArgumentOutOfRangeException("Value must be one of the built in Recipe.CraftingType values.");
				_permenantCraftingStationType = value;
			}
		}

		Recipe.CraftingType _customCraftingType;
		public Recipe.CraftingType CustomCraftingType {
			get => _customCraftingType;
			internal set
            {
				if (value.IsDefinedValue())
					throw new ArgumentOutOfRangeException("Value must not be one of the built in Recipe.CraftingType values.");
				_customCraftingType = value;
            } 
		}

		protected Dictionary<Recipe.CraftingType, Sprite> _craftingBackgrounds = new Dictionary<Recipe.CraftingType, Sprite>();

		#region Reflected CraftingMenu Fields
		protected CraftingStation _craftingStation
		{
			get => this.GetPrivateField<CraftingMenu, CraftingStation>("m_craftingStation");
			set => this.SetPrivateField<CraftingMenu, CraftingStation>("m_craftingStation", value);
		}
		protected UnityEngine.UI.Text _lblStationName
		{
			get => this.GetPrivateField<CraftingMenu, UnityEngine.UI.Text>("m_lblStationName");
			set => this.SetPrivateField<CraftingMenu, UnityEngine.UI.Text>("m_lblStationName", value);
		}
		protected bool _simpleMode
		{
			get => this.GetPrivateField<CraftingMenu, bool>("m_simpleMode");
			set => this.SetPrivateField<CraftingMenu, bool>("m_simpleMode", value);
		}
		protected Recipe.CraftingType _craftingStationType
		{
			get => this.GetPrivateField<CraftingMenu, Recipe.CraftingType>("m_craftingStationType");
			set => this.SetPrivateField<CraftingMenu, Recipe.CraftingType>("m_craftingStationType", value);
		}
		protected Sprite _survivalCraftingBg
		{
			get => this.GetPrivateField<CraftingMenu, Sprite>("m_survivalCraftingBg");
			set => this.SetPrivateField<CraftingMenu, Sprite>("m_survivalCraftingBg", value);
		}
		protected Sprite _cookingPotCraftingBg
		{
			get => this.GetPrivateField<CraftingMenu, Sprite>("m_cookingPotCraftingBg");
			set => this.SetPrivateField<CraftingMenu, Sprite>("m_cookingPotCraftingBg", value);
		}
		protected Sprite _cookingFireCraftingBg
		{
			get => this.GetPrivateField<CraftingMenu, Sprite>("m_cookingFireCraftingBg");
			set => this.SetPrivateField<CraftingMenu, Sprite>("m_cookingFireCraftingBg", value);
		}
		protected Sprite _alchemyCraftingBg
		{
			get => this.GetPrivateField<CraftingMenu, Sprite>("m_alchemyCraftingBg");
			set => this.SetPrivateField<CraftingMenu, Sprite>("m_alchemyCraftingBg", value);
		}
		protected UnityEngine.UI.Image _imgCraftingBackground
		{
			get => this.GetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_imgCraftingBackground");
			set => this.SetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_imgCraftingBackground", value);
		}
		protected UnityEngine.UI.Image _singleIngredientBackground
		{
			get => this.GetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_singleIngredientBackground");
			set => this.SetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_singleIngredientBackground", value);
		}
		protected UnityEngine.UI.Image _multipleIngrenentsBrackground
		{
			get => this.GetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_multipleIngrenentsBrackground");
			set => this.SetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_multipleIngrenentsBrackground", value);
		}
		protected IngredientSelector[] _ingredientSelectors
		{
			get => this.GetPrivateField<CraftingMenu, IngredientSelector[]>("m_ingredientSelectors");
			set => this.SetPrivateField<CraftingMenu, IngredientSelector[]>("m_ingredientSelectors", value);
		}
		protected UnityEngine.UI.Text _lblFreeRecipeDescription
		{
			get => this.GetPrivateField<CraftingMenu, UnityEngine.UI.Text>("m_lblFreeRecipeDescription");
			set => this.SetPrivateField<CraftingMenu, UnityEngine.UI.Text>("m_lblFreeRecipeDescription", value);
		}
		protected DictionaryExt<int, CompatibleIngredient> _availableIngredients
		{
			get => this.GetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients");
			set => this.SetPrivateField<CraftingMenu, DictionaryExt<int, CompatibleIngredient>>("m_availableIngredients", value);
		}

		protected static string[] _freeRecipesLocKey
		{
			get => typeof(CraftingMenu).GetField("m_freeRecipesLocKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
										.GetValue(null) as string[];
			set => typeof(CraftingMenu).GetField("m_freeRecipesLocKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
										.SetValue(null, value);
		}
		protected List<Recipe> _allRecipes
		{
			get => this.GetPrivateField<CraftingMenu, List<Recipe>>("m_allRecipes");
			set => this.SetPrivateField<CraftingMenu, List<Recipe>>("m_allRecipes", value);
		}
		protected bool _refreshComplexeRecipeRequired
		{
			get => this.GetPrivateField<CraftingMenu, bool>("m_refreshComplexeRecipeRequired");
			set => this.SetPrivateField<CraftingMenu, bool>("m_refreshComplexeRecipeRequired", value);
		}
		#endregion

		#region Reflected CraftingMenu Methods
		protected Action resetFreeRecipeLastIngredients => () => this.InvokePrivateMethod<CraftingMenu>("ResetFreeRecipeLastIngredients");
		protected Action refreshAutoRecipe => () => this.InvokePrivateMethod<CraftingMenu>("RefreshAutoRecipe");
        #endregion

		public CustomCraftingMenu()
        {
            CraftingMenuPatches.RefreshAvailableIngredientsOverridden += RefreshAvailableIngredientsOverride;

		}
		public bool IsCustomCraftingStation() => PermanentCraftingStationType == null && CustomCraftingType != default;
        private bool RefreshAvailableIngredientsOverride(CraftingMenu craftingMenu)
        {
			if (!(craftingMenu is CustomCraftingMenu))
				return false;

			_availableIngredients.Values.ForEach(i => i.Clear());

			Tag craftingIngredient = TagSourceManager.GetCraftingIngredient(GetRecipeCraftingType());
			var availableIngredients = _availableIngredients;
			base.LocalCharacter.Inventory.InventoryIngredients(craftingIngredient, ref availableIngredients);
			_availableIngredients = availableIngredients;

			return true;
		}
		public Recipe.CraftingType GetRecipeCraftingType()
        {
			//Priority order, Permanent > Custom > builtin m_craftingStationType
			return PermanentCraftingStationType ?? (CustomCraftingType.IsDefinedValue() ? _craftingStationType : CustomCraftingType);
		}
        protected override void AwakeInit()
		{
			Logger.LogDebug($"CustomCraftingMenu::AwakeInit() called on type {this.GetType().Name}");
			try
			{
				RemoveClones();

				if (PermanentCraftingStationType != null)
				{
					_craftingStationType = (Recipe.CraftingType)PermanentCraftingStationType;
					Logger.LogDebug($"CustomCraftingMenu::AwakeInit(): Set m_craftingStationType to {PermanentCraftingStationType}. Current value: {_craftingStationType}");
				}

				//if (_initCraftingTypeSet)
				//{
				//	_craftingStationType = InitCraftingType;
				//	Logger.LogDebug($"CustomCraftingMenu::AwakeInit(): Set m_craftingStationType to {InitCraftingType}. Current value: {_craftingStationType}");
				//}

				if (!BypassCraftingMenuAwakeInit)
					base.AwakeInit();
				else
				{

					Logger.LogDebug($"CustomCraftingMenu::AwakeInit() Bypassing CraftingMenu AwakeInit()");

					var awakePtr = typeof(Panel).GetMethod("AwakeInit", BindingFlags.Instance | BindingFlags.NonPublic)
						.MethodHandle.GetFunctionPointer();
					var menuPanelAwakeInit = (Action)Activator.CreateInstance(typeof(Action), this, awakePtr);
					menuPanelAwakeInit.Invoke();
				}
			}
			catch (Exception ex)
			{
				Logger.LogException($"CustomCraftingMenu::AwakeInit() Exception.\n", ex);
			}
		}
		private void RemoveClones()
        {
			const string clonedSelectorPath = "Content/Ingredients/IngredientSelector(Clone)";
			const string clonedFreeRecipePath = "LeftPanel/Scroll View/Viewport/Content/_freeRecipe";

			//Remove these because they will be readded in awakeinit
			var clonedSelector = this.gameObject.transform.Find(clonedSelectorPath);
			while (clonedSelector?.gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(clonedSelector.gameObject);
				clonedSelector = this.gameObject.transform.Find(clonedSelectorPath);
			}
			var clonedFreeRecipe = this.gameObject.transform.Find(clonedFreeRecipePath);
			while (clonedFreeRecipe?.gameObject != null)
            {
				UnityEngine.Object.DestroyImmediate(clonedFreeRecipe.gameObject);
				clonedFreeRecipe = this.gameObject.transform.Find(clonedFreeRecipePath);
			}
		}
		private bool showRecurseCheck = false;
        protected override void StartInit()
        {
			//flip the craftingStationType over to a custom one (if configured), so that stations
			//recipes are retrieved. (Last thing CraftingMenu.StartInit() does is setting all recipes
			//code: m_allRecipes = RecipeManager.Instance.GetRecipes(m_craftingStationType, base.LocalCharacter);
			var craftingStationType = _craftingStationType;
			_craftingStationType = GetRecipeCraftingType();
			base.StartInit();
			//Reset it back to the original.
			_craftingStationType = craftingStationType;
        }
        public override void Show()
        {
            try
            {
#if !DEBUG
				Logger.LogDebug($"CustomCraftingMenu::Show(): Type is {this.GetType().Name}");
				if (PermenantCraftingStationType != null
					&& PermenantCraftingStationType.Value.IsDefinedValue()
					&& !showRecurseCheck)
				{
					Logger.LogDebug($"CustomCraftingMenu::Show() {this.GetType().Name} is a permenant {PermenantCraftingStationType} crafting station.");
					showRecurseCheck = true;
					Show((Recipe.CraftingType)PermenantCraftingStationType);
					return;
				}
				showRecurseCheck = false;

				Logger.LogDebug($"CustomCraftingMenu::AwakeInit() Bypassing CraftingMenu Show()");
				var awakePtr = typeof(MenuPanel).GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MethodHandle.GetFunctionPointer();
                var menuPanelAwakeInit = (Action)Activator.CreateInstance(typeof(Action), this, awakePtr);
                menuPanelAwakeInit.Invoke();

				CustomShow();
#else
				Logger.LogDebug($"CustomCraftingMenu::Show(): Type is {this.GetType().Name}");
				
				if (PermanentCraftingStationType != null
					&& PermanentCraftingStationType.Value.IsDefinedValue()
					&& !showRecurseCheck)
				{
					Logger.LogDebug($"CustomCraftingMenu::Show() {this.GetType().Name} is a permenant {PermanentCraftingStationType} crafting station.");
					showRecurseCheck = true;
					Show((Recipe.CraftingType)PermanentCraftingStationType);
					return;
				}
				showRecurseCheck = false;

				this.SetPrivateField<Panel, bool>("m_showWanted", true);
				Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking UIElement_Show()");
				UIElement_Show();
				Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking Panel_Show()");
				Panel_Show();
				Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking MenuPanel_Show()");
				MenuPanel_Show();
				Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking CustomShow()");
				CustomShow();
#endif
			}
			catch (Exception ex)
            {
                Logger.LogException($"CustomCraftingMenu::Show() Exception.\n", ex);
            }
        }
		private void Show(Recipe.CraftingType craftingStationType)
		{
			if (craftingStationType != Recipe.CraftingType.Alchemy
				&& craftingStationType != Recipe.CraftingType.Cooking
				&& craftingStationType != Recipe.CraftingType.Survival)
            {
				throw new ArgumentException($"Invalid CraftingType. Allowed CraftingTypes are Cooking, Alchemy and Survival.", nameof(craftingStationType));
            }

			const int stationaryAlchemyStation = 1900003;
			const int stationaryCookingStation = 1900004;

			var stationId = stationaryAlchemyStation;
			if (craftingStationType == Recipe.CraftingType.Cooking)
				stationId = stationaryCookingStation;

			var stationItem = ResourcesPrefabManager.Instance.GetItemPrefab(stationId);
			if (stationItem == null)
				stationItem = ResourcesPrefabManager.Instance.GenerateItem(stationId.ToString());

			Show(stationItem.gameObject.GetComponentInChildren<CraftingStation>());
		}
		private void CustomShow()
        {
			Logger.LogDebug($"CustomShow: 0");
			

			Logger.LogDebug($"CustomShow: 1");
			if (_craftingStation == null)
			{
				//if (_craftingStationType <= Enum.GetValues(typeof(Recipe.CraftingType)).Cast<Recipe.CraftingType>().Max())
				_craftingStationType = Recipe.CraftingType.Survival;

				Logger.LogDebug($"CustomShow: 1:A:1");
				if ((bool)(UnityEngine.Object)(object)_lblStationName)
				{
					_lblStationName.text = "";
				}
				_simpleMode = false;
				Logger.LogDebug($"CustomShow: 1:A:2");
			}
			else
			{
				Logger.LogDebug($"CustomShow: 1:B:1");
				if ((bool)(UnityEngine.Object)(object)_lblStationName)
				{
					_lblStationName.text = _craftingStation.DisplayName;
				}
				_simpleMode = !_craftingStation.AllowComplexRecipe;
				Logger.LogDebug($"CustomShow: 1:B:2");
			}
			Logger.LogDebug($"CustomShow: 2");
			if ((bool)(UnityEngine.Object)(object)_singleIngredientBackground)
			{
				((UnityEngine.UI.Graphic)(object)_singleIngredientBackground).SetAlpha(_simpleMode ? 1 : 0);
			}
			Logger.LogDebug($"CustomShow: 3");
			if ((bool)(UnityEngine.Object)(object)_multipleIngrenentsBrackground)
			{
				((UnityEngine.UI.Graphic)(object)_multipleIngrenentsBrackground).SetAlpha((!_simpleMode) ? 1 : 0);
			}
			Logger.LogDebug($"CustomShow: 4");
			for (int i = 1; i < _ingredientSelectors.Length; i++)
			{
				_ingredientSelectors[i].Show(!_simpleMode);
			}
			Logger.LogDebug($"CustomShow: 5");
			int num = -1;
			Sprite overrideSprite = null;
			Logger.LogDebug($"CustomShow: 6");
			switch (_craftingStationType)
			{
				case Recipe.CraftingType.Survival:
					num = 1;
					overrideSprite = _survivalCraftingBg;
					break;
				case Recipe.CraftingType.Cooking:
					if (_craftingStation.AllowComplexRecipe)
					{
						num = 3;
						overrideSprite = _cookingPotCraftingBg;
					}
					else
					{
						num = 2;
						overrideSprite = _cookingFireCraftingBg;
					}
					break;
				case Recipe.CraftingType.Alchemy:
					num = 0;
					overrideSprite = _alchemyCraftingBg;
					break;
				case Recipe.CraftingType.Forge:
					num = 4;
					overrideSprite = _alchemyCraftingBg;
					break;
				default:
					num = 1;
					overrideSprite = _craftingBackgrounds.TryGetValue(_craftingStationType, out var customSprite) ? 
						customSprite : _survivalCraftingBg;
					break;

			}
			Logger.LogDebug($"CustomShow: 7");

			Logger.LogDebug($"CustomShow: 8");
			if ((bool)(UnityEngine.Object)(object)_lblFreeRecipeDescription)
			{
				_lblFreeRecipeDescription.text = LocalizationManager.Instance.GetLoc(_freeRecipesLocKey[num]);
			}
			Logger.LogDebug($"CustomShow: 9");
			if ((bool)(UnityEngine.Object)(object)_imgCraftingBackground)
			{
				_imgCraftingBackground.overrideSprite = overrideSprite;
			}
			Logger.LogDebug($"CustomShow: 10");
			//ResetFreeRecipeLastIngredients();
			resetFreeRecipeLastIngredients();

			Logger.LogDebug($"CustomShow: 11");
			//m_allRecipes = RecipeManager.Instance.GetRecipes(m_craftingStationType, base.LocalCharacter);
			Logger.LogDebug($"CustomCraftingMenu::CustomShow(): Getting Recipes for CraftingType {GetRecipeCraftingType()} and character '{base.LocalCharacter.UID}'.");
			_allRecipes =  RecipeManager.Instance.GetRecipes(GetRecipeCraftingType(), base.LocalCharacter);

			Logger.LogDebug($"CustomShow: 12");
			//m_refreshComplexeRecipeRequired = true;
			_refreshComplexeRecipeRequired = true;

			Logger.LogDebug($"CustomShow: 13");
			//RefreshAutoRecipe();
			refreshAutoRecipe();

			Logger.LogDebug($"CustomShow: 14");
			OnRecipeSelected(-1, _forceRefresh: true);
		}
        #region Copies of Base Methods for Debugging 
#if DEBUG
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
			var m_lblStationName = this.GetPrivateField<CraftingMenu, UnityEngine.UI.Text>("m_lblStationName");
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
			var m_singleIngredientBackground = this.GetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_singleIngredientBackground");
			if ((bool)(UnityEngine.Object)(object)m_singleIngredientBackground)
			{
				((UnityEngine.UI.Graphic)(object)m_singleIngredientBackground).SetAlpha(m_simpleMode ? 1 : 0);
			}
			Logger.LogDebug($"CraftingMenu_Show: 3");
			var m_multipleIngrenentsBrackground = this.GetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_multipleIngrenentsBrackground");
			if ((bool)(UnityEngine.Object)(object)m_multipleIngrenentsBrackground)
			{
				((UnityEngine.UI.Graphic)(object)m_multipleIngrenentsBrackground).SetAlpha((!m_simpleMode) ? 1 : 0);
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
			var m_lblFreeRecipeDescription = this.GetPrivateField<CraftingMenu, UnityEngine.UI.Text>("m_lblFreeRecipeDescription");
			var m_freeRecipesLocKey = typeof(CraftingMenu).GetField("m_freeRecipesLocKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
				.GetValue(null) as string[];
			

			var m_imgCraftingBackground = this.GetPrivateField<CraftingMenu, UnityEngine.UI.Image>("m_imgCraftingBackground");
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
#endif
		#endregion
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
