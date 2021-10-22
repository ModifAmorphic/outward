using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Modules.Crafting.Models;
using ModifAmorphic.Outward.Modules.Crafting.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CustomCraftingMenu : CraftingMenu
    {
		public CustomCraftingModule ParentCraftingModule { get; private set; }

		protected virtual string ModId { get; set; }

		private static IModifLogger nullLogger = new NullLogger();

		protected IModifLogger Logger => LoggerFactory.Invoke();
		private Func<IModifLogger> _loggerFactory = () => nullLogger;
		protected Func<IModifLogger> LoggerFactory
		{
			get => _loggerFactory;
			set => _loggerFactory = value;
		}
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

		//private Tag _inventoryFilterTag;
		///// <summary>
		///// Used to filter out items from inventory for crafting recipes. Uses this tag and the crafting ingredient tag. Defaults to
		///// the crafting station, but can be set in an inherited class to any tag.
		///// </summary>
		//public Tag InventoryFilterTag
  //      {
		//	get => _inventoryFilterTag;
		//	protected set => _inventoryFilterTag = value;
  //      }
		private bool _hideFreeCraftingRecipe = false;
		public bool HideFreeCraftingRecipe
		{
			get => _hideFreeCraftingRecipe;
			protected set => _hideFreeCraftingRecipe = value;
		}

		//private bool _includeEnchantedIngredients = false;
		///// <summary>
		///// Whether or not to include items that have been enchanted in the list of available ingredients for recipes. The 
		///// base game code excludes all enchanted items from results.
		///// </summary>
		//public bool IncludeEnchantedIngredients
		//{
		//	get => _includeEnchantedIngredients;
		//	protected set => _includeEnchantedIngredients = value;
		//}

		public IngredientCraftData IngredientCraftData = new IngredientCraftData();

		protected Dictionary<Recipe.CraftingType, Sprite> _craftingBackgrounds = new Dictionary<Recipe.CraftingType, Sprite>();

		private int[] _selectorIngredientMap = null;

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

		internal IngredientSelector _ingredientSelectorTemplate
		{
			get => this.GetPrivateField<CraftingMenu, IngredientSelector>("m_ingredientSelectorTemplate");
			set => this.SetPrivateField<CraftingMenu, IngredientSelector>("m_ingredientSelectorTemplate", value);
		}
		internal IngredientSelector[] _ingredientSelectors
		{
			get => this.GetPrivateField<CraftingMenu, IngredientSelector[]>("m_ingredientSelectors");
			set => this.SetPrivateField<CraftingMenu, IngredientSelector[]>("m_ingredientSelectors", value);
		}
		private List<int> _ingredientSelectorCachedItems
		{
			get => this.GetPrivateField<CraftingMenu, List<int>>("ingredientSelectorCachedItems");
			set => this.SetPrivateField<CraftingMenu, List<int>>("ingredientSelectorCachedItems", value);
		}
		private ItemListSelector _tempSelectorWindow
		{
			get => this.GetPrivateField<CraftingMenu, ItemListSelector>("m_tempSelectorWindow");
			set => this.SetPrivateField<CraftingMenu, ItemListSelector>("m_tempSelectorWindow", value);
		}
		private Transform _selectorWindowPos
		{
			get => this.GetPrivateField<CraftingMenu, Transform>("m_selectorWindowPos");
			set => this.SetPrivateField<CraftingMenu, Transform>("m_selectorWindowPos", value);
		}
		internal int[] _lastFreeRecipeIngredientIDs
		{
			get => this.GetPrivateField<CraftingMenu, int[]>("m_lastFreeRecipeIngredientIDs");
			set => this.SetPrivateField<CraftingMenu, int[]>("m_lastFreeRecipeIngredientIDs", value);
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
		protected List<KeyValuePair<int, Recipe>> _complexeRecipes
		{
			get => this.GetPrivateField<CraftingMenu, List<KeyValuePair<int, Recipe>>>("m_complexeRecipes");
			set => this.SetPrivateField<CraftingMenu, List<KeyValuePair<int, Recipe>>>("m_complexeRecipes", value);
		}
		protected bool _refreshComplexeRecipeRequired
		{
			get => this.GetPrivateField<CraftingMenu, bool>("m_refreshComplexeRecipeRequired");
			set => this.SetPrivateField<CraftingMenu, bool>("m_refreshComplexeRecipeRequired", value);
		}
		
		protected int _lastFreeRecipeIndex
		{
			get => this.GetPrivateField<CraftingMenu, int>("m_lastFreeRecipeIndex");
			set => this.SetPrivateField<CraftingMenu, int>("m_lastFreeRecipeIndex", value);
		}
		protected int _lastRecipeIndex
        {
			get => this.GetPrivateField<CraftingMenu, int>("m_lastRecipeIndex");
			set => this.SetPrivateField<CraftingMenu, int>("m_lastRecipeIndex", value);
		}
		protected List<RecipeDisplay> _recipeDisplays
		{
			get => this.GetPrivateField<CraftingMenu, List<RecipeDisplay>>("m_recipeDisplays");
			set => this.SetPrivateField<CraftingMenu, List<RecipeDisplay>>("m_recipeDisplays", value);
		}
		internal RecipeResultDisplay _recipeResultDisplay
		{
			get => this.GetPrivateField<CraftingMenu, RecipeResultDisplay>("m_recipeResultDisplay");
			set => this.SetPrivateField<CraftingMenu, RecipeResultDisplay>("m_recipeResultDisplay", value);
		}
		protected RecipeDisplay _freeRecipeDisplay
		{
			get => this.GetPrivateField<CraftingMenu, RecipeDisplay>("m_freeRecipeDisplay");
			set => this.SetPrivateField<CraftingMenu, RecipeDisplay>("m_freeRecipeDisplay", value);
		}
		protected ItemDetailsDisplay _itemDetailPanel
		{
			get => this.GetPrivateField<CraftingMenu, ItemDetailsDisplay>("m_itemDetailPanel");
			set => this.SetPrivateField<CraftingMenu, ItemDetailsDisplay>("m_itemDetailPanel", value);
		}
		protected GameObject _freeRecipeDescriptionPanel
		{
			get => this.GetPrivateField<CraftingMenu, GameObject>("m_freeRecipeDescriptionPanel");
			set => this.SetPrivateField<CraftingMenu, GameObject>("m_freeRecipeDescriptionPanel", value);
		}
		
		#endregion

		#region Reflected CraftingMenu Methods
		internal Action resetFreeRecipeLastIngredients => () => this.InvokePrivateMethod<CraftingMenu>("ResetFreeRecipeLastIngredients");
		protected Action refreshAutoRecipe => () => this.InvokePrivateMethod<CraftingMenu>("RefreshAutoRecipe");
		protected Action cancelCrafting => () => this.InvokePrivateMethod<CraftingMenu>("CancelCrafting");
		protected Action<bool> setCraftButtonEnable => (show) => this.InvokePrivateMethod<CraftingMenu>("SetCraftButtonEnable", show);
		protected Action refreshFreeRecipeResult => () => this.InvokePrivateMethod<CraftingMenu>("RefreshFreeRecipeResult");
		protected Action closeItemSelectionWindow => () => this.InvokePrivateMethod<CraftingMenu>("CloseItemSelectionWindow");
		#endregion

		public CustomCraftingMenu()
        {
			//CraftingMenuPatches.OnRecipeSelectedAfter += (args) => RefreshResult();
            //CraftingMenuPatches.OnRecipeSelectedAfter += CraftingMenuPatches_OnRecipeSelectedAfter;
			//CraftingMenuPatches.IngredientSelectorHasChangedAfter += (args) => RefreshResult();
		}

		//internal void OnRecipeSelectedBeforeBase(int index, bool forceRefresh)
  //      {
		//	//ParentCraftingModule.RecipeDisplayService.ResetSlots(this);
		//}

		public bool TryOnIngredientSelectorClicked(int selectorIndex)
        {
			IngredientSelector ingredientSelector = _ingredientSelectors[selectorIndex];
			var ingredientSelectorCachedItems = _ingredientSelectorCachedItems;
			ingredientSelectorCachedItems.Clear();
			var availableIngredients = _availableIngredients;
			var ingredientIndex = _selectorIngredientMap[selectorIndex];
			var lastRecipeIndex = _lastRecipeIndex;
			if (lastRecipeIndex != -1)
			{
				if (_complexeRecipes[lastRecipeIndex].Value.Ingredients[ingredientIndex].ActionType == RecipeIngredient.ActionTypes.AddGenericIngredient)
				{
					IList<int> stepCompatibleIngredients = _recipeDisplays[lastRecipeIndex].GetStepCompatibleIngredients(ingredientIndex);
					for (int i = 0; i < stepCompatibleIngredients.Count; i++)
					{
						if (availableIngredients.TryGetValue(stepCompatibleIngredients[i], out var compatibleIngredient) && compatibleIngredient.AvailableQty > 0)
						{
							ingredientSelectorCachedItems.Add(compatibleIngredient.ItemID);
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < availableIngredients.Count; i++)
				{
					if (availableIngredients.Values[i].AvailableQty > 0 || ingredientSelector.AssignedIngredient == availableIngredients.Values[i])
					{
						ingredientSelectorCachedItems.Add(availableIngredients.Values[i].ItemID);
					}
				}
			}
			if (ingredientSelectorCachedItems.Count <= 0)
			{
				return true;
			}
			_tempSelectorWindow = m_characterUI.GetListSelector();
			var tempSelectorWindow = _tempSelectorWindow;
			if ((bool)_tempSelectorWindow)
			{
				tempSelectorWindow.onItemHovered = RefreshItemDetailDisplay;
				tempSelectorWindow.onItemClicked = delegate (IItemDisplay _display)
				{
					IngredientSelectorChanged(selectorIndex, _display?.RefItem.ItemID ?? (-1));
					closeItemSelectionWindow();
				};
				tempSelectorWindow.Position = (_selectorWindowPos ? _selectorWindowPos.position : base.transform.position);
				tempSelectorWindow.Title = ingredientSelector.Text;
				tempSelectorWindow.ShowSelector(ingredientSelectorCachedItems, ingredientSelector.gameObject, lastRecipeIndex == -1);
			}
			return true;
		}

		private void IngredientSelectorChanged(int selectorIndex, int itemID)
        {
			_availableIngredients.TryGetValue(itemID, out var _outValue);
			var ingredientIndex = _selectorIngredientMap[selectorIndex];
			var ingredientSelectors = _ingredientSelectors;
			RecipeIngredient recipeIngredient = ((_lastRecipeIndex != -1) ? _complexeRecipes[_lastRecipeIndex].Value.Ingredients[ingredientIndex] : null);
			ingredientSelectors[selectorIndex].Set(recipeIngredient, _outValue);
			if (_lastRecipeIndex != -1)
			{
				return;
			}
			refreshFreeRecipeResult();
			bool craftButtonEnable = false;
			for (int i = 0; i < ingredientSelectors.Length; i++)
			{
				if (ingredientSelectors[i].AssignedIngredient != null)
				{
					craftButtonEnable = true;
				}
			}
			setCraftButtonEnable(craftButtonEnable);
		}

		public bool OnRecipeSelectedOverride(int _index, bool _forceRefresh = false)
		{
			if (!ParentCraftingModule.RecipeDisplayService.TryGetDisplayConfig(this.GetType(), out var displayConfig)
				|| !(displayConfig.StaticIngredients?.Count > 0)
				|| (_lastRecipeIndex == _index && !_forceRefresh)
				|| _index == -1)
				return false;

			var ingredientSelectors = _ingredientSelectors;

			if (IsCraftingInProgress)
			{
				cancelCrafting();
			}
			if (_lastRecipeIndex != -1)
			{
				_recipeDisplays[_lastRecipeIndex].SetHighlight(_highlight: false);
			}
			else
			{
				_freeRecipeDisplay.SetHighlight(_highlight: false);
			}
			for (int i = 0; i < ingredientSelectors.Length; i++)
			{
				ingredientSelectors[i].Free(_resetUseCount: true);
			}
			_lastRecipeIndex = _index;
			_itemDetailPanel.Show(_lastRecipeIndex != -1);
			_freeRecipeDescriptionPanel.gameObject.SetActive(_lastRecipeIndex == -1);

			resetFreeRecipeLastIngredients();
			setCraftButtonEnable(_recipeDisplays[_index].IsRecipeIngredientsComplete);
			_recipeResultDisplay.SetRecipeResult(_complexeRecipes[_index].Value.Results[0]);
			RefreshItemDetailDisplay(_recipeResultDisplay);

			var recipeIngredients = _complexeRecipes[_index].Value.Ingredients;
			var staticIngredients = displayConfig.StaticIngredients.ToDictionary(s => s.IngredientID, s => s);

			for (int i = 0; i < _selectorIngredientMap.Length; i++)
				_selectorIngredientMap[i] = -1;

			for (int ingIndex = 0; ingIndex < _selectorIngredientMap.Length; ingIndex++)
			{
				Logger.LogDebug($"OnRecipeSelectedOverride:: ingIndex:{ingIndex}, recipeIngredients.Length: {recipeIngredients.Length}");
				if (recipeIngredients.Length > ingIndex)
				{
					Logger.LogDebug($"OnRecipeSelectedOverride:: recipeIngredients[ingIndex] is CustomRecipeIngredient: {recipeIngredients[ingIndex] is CustomRecipeIngredient}");
					if (recipeIngredients[ingIndex] is CustomRecipeIngredient)
						Logger.LogDebug($"staticIngredients.TryGetValue(customIngredient.CustomRecipeIngredientID, out var staticConfig): {staticIngredients.TryGetValue(((CustomRecipeIngredient)recipeIngredients[ingIndex]).CustomRecipeIngredientID, out _)}");
				}

				if (recipeIngredients.Length > ingIndex && 
					recipeIngredients[ingIndex] is CustomRecipeIngredient customIngredient &&
					staticIngredients.TryGetValue(customIngredient.CustomRecipeIngredientID, out var staticConfig))
				{
					int staticPosition = (int)staticConfig.IngredientSlotPosition;
					_selectorIngredientMap[staticPosition] = ingIndex;
				}
			}

			var ingredIndex = 0;
			for (int slot = 0; slot < _selectorIngredientMap.Length; slot++)
            {
				if (_selectorIngredientMap[slot] == -1)
                {
					while (_selectorIngredientMap.Contains(ingredIndex))
						ingredIndex++;
					if (ingredIndex < recipeIngredients.Length)
					{
						_selectorIngredientMap[slot] = ingredIndex;
						ingredIndex++;
					}
				}
			}
			var recipeDisplays = _recipeDisplays;
			var availableIngredients = _availableIngredients;
			for (int slot = 0; slot < _selectorIngredientMap.Length; slot++)
			{
				Logger.LogDebug($"OnRecipeSelectedOverride:: Slot {slot} set to IngredientIndex {_selectorIngredientMap[slot]}");
				var ingredientIndex = _selectorIngredientMap[slot];

				if (ingredientIndex > -1)
				{
					if (slot < ingredientSelectors.Length - 1)
					{
						if (ingredientSelectors[slot + 1].Button.interactable)
						{
							ingredientSelectors[slot].Button.SetRightNav(ingredientSelectors[slot + 1].Button);
						}
						else
						{
							ingredientSelectors[slot].Button.SetRightNav(null);
						}
					}
                    ingredientSelectors[slot].Button.interactable = (true);
					int bestIngredientIndex = recipeDisplays[_index].BestIngredientsPerStep[ingredientIndex];
					var bestIngredient = bestIngredientIndex != -1 ? availableIngredients[bestIngredientIndex] : null;
					if (bestIngredient != null 
							&& bestIngredient is CustomCompatibleIngredient ccIng
							&& recipeIngredients[ingredientIndex] is CustomRecipeIngredient crIng)
                    {
						ccIng.StaticIngredientID = crIng.CustomRecipeIngredientID;
					}

					ingredientSelectors[slot].Set(recipeIngredients[ingredientIndex], bestIngredient);
				}
				else
				{
					ingredientSelectors[slot].Clear();
                    ingredientSelectors[slot].Button.interactable = (false);
				}
			}
			recipeDisplays[_index].SetHighlight(_highlight: true);
			return true;
		}
		//internal void OnRecipeSelectedAfterBase(int index, bool forceRefresh)
  //      {
		//	if (index != -1)
		//	{
		//		Logger.LogDebug($"OnRecipeSelected: index: {index}");
		//		ParentCraftingModule.RecipeDisplayService.SortIngredientSlots(this, _complexeRecipes[index].Value);
		//	}
		//	RefreshResult();
		//}
		internal new void IngredientSelectorHasChanged(int selectorIndex, int itemID)
        {
			RefreshResult();
		}

		internal void RefreshResult()
        {
			try
			{
				if (!CanCustomCraft())
					return;

				((CustomRecipeResultDisplay)_recipeResultDisplay).SetCustomRecipeResult(GetSelectedRecipe().Results[0]);
			}
			catch (Exception ex)
            {
				Logger.LogException($"CustomCraftingMenu::RefreshResult() Exception.\n", ex);
			}
        }

		public bool CanCustomCraft()
        {
			var recipe = GetSelectedRecipe();
			var ingredients = GetSelectedIngredients();
			if (!(recipe != null
				&& recipe.Results.Any()
				&& recipe.Results[0] is DynamicCraftingResult
				&& ingredients != null
				&& ingredients.Any()
				&& _recipeResultDisplay is CustomRecipeResultDisplay))
            {
				//Logger.LogTrace($"{nameof(CustomCraftingMenu)}::{nameof(CanCustomCraft)}: Result: false\n" +
				//	$"\trecipe != null? {recipe != null}\n" +
				//	$"\trecipe.Results.Any()? {recipe?.Results?.Any()}\n" +
				//	$"\tingredients != null? {ingredients != null}\n" +
				//	$"\tingredients.Any()? {ingredients?.Any()}\n" +
				//	$"\t_recipeResultDisplay is CustomRecipeResultDisplay? { _recipeResultDisplay is CustomRecipeResultDisplay}");
				return false;
            }
			return true;
		}
        public bool IsCustomCraftingStation() => PermanentCraftingStationType == null && CustomCraftingType != default;

		public Recipe.CraftingType GetRecipeCraftingType()
        {
			//Priority order, Permanent > Custom > builtin m_craftingStationType
			return PermanentCraftingStationType ?? (CustomCraftingType.IsDefinedValue() ? _craftingStationType : CustomCraftingType);
		}
        protected override void AwakeInit()
		{
			Logger.LogDebug($"CustomCraftingMenu::AwakeInit() called on type {this.GetType().Name}");
			if (string.IsNullOrEmpty(ModId))
				throw new InvalidOperationException($"AwakeInit() was invoked on type {this.GetType()} before property {nameof(ModId)} was set. " +
					$"A CustomCraftingMenu must have it's {nameof(ModId)} configured before it can be used.");
			try
			{
				ParentCraftingModule = ModifModules.GetCustomCraftingModule(ModId);
				if (PermanentCraftingStationType != null)
				{
					_craftingStationType = (Recipe.CraftingType)PermanentCraftingStationType;
					Logger.LogDebug($"CustomCraftingMenu::AwakeInit(): Set m_craftingStationType to {PermanentCraftingStationType}. Current value: {_craftingStationType}");
				}

				base.AwakeInit();

				_selectorIngredientMap = new int[_ingredientSelectors.Length];
				//else
				//{

				//	Logger.LogDebug($"CustomCraftingMenu::AwakeInit() Bypassing CraftingMenu AwakeInit()");

				//	var awakePtr = typeof(Panel).GetMethod("AwakeInit", BindingFlags.Instance | BindingFlags.NonPublic)
				//		.MethodHandle.GetFunctionPointer();
				//	var menuPanelAwakeInit = (Action)Activator.CreateInstance(typeof(Action), this, awakePtr);
				//	menuPanelAwakeInit.Invoke();
				//}
			}
			catch (Exception ex)
			{
				Logger.LogException($"CustomCraftingMenu::AwakeInit() Exception.", ex);
			}
		}
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
			//if (!_inventoryFilterTag.IsSet)
			//	_inventoryFilterTag = TagSourceManager.GetCraftingIngredient(GetRecipeCraftingType());
			if (HideFreeCraftingRecipe)
			{
				this._freeRecipeDisplay.gameObject.SetActive(false);
			}
		}
		

		private bool showRecurseCheck = false;
		public override void Show()
        {
            try
            {
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

				//_allRecipes = RecipeManager.Instance.GetRecipes(GetRecipeCraftingType(), base.LocalCharacter);

				//enable this, disable base.Show() to debug menu Show() methods
				//DebugShow()
				base.Show();

				resetFreeRecipeLastIngredients();

				Logger.LogDebug($"CustomCraftingMenu::CustomShow(): Getting Recipes for CraftingType {GetRecipeCraftingType()} and character '{base.LocalCharacter.UID}'.");
				var allRecipes = RecipeManager.Instance.GetRecipes(GetRecipeCraftingType(), base.LocalCharacter);
				ParentCraftingModule.RecipeDisplayService.AddStaticIngredients(this, ref allRecipes);
				_allRecipes = allRecipes;
				_refreshComplexeRecipeRequired = true;
				refreshAutoRecipe();
				OnRecipeSelected(-1, _forceRefresh: true);
            }
			catch (Exception ex)
            {
                Logger.LogException($"CustomCraftingMenu::Show() Exception.\n", ex);
            }

			ParentCraftingModule.RecipeDisplayService.PositionSelectors(this, _ingredientSelectors);
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


		//public Recipe GetSelectedRecipe() =>
		//		_lastRecipeIndex != -1 ? _complexeRecipes[_lastRecipeIndex].Value : !HideFreeCraftingRecipe ? _complexeRecipes[_lastFreeRecipeIndex].Value : null;
		public Recipe GetSelectedRecipe() =>
				_lastRecipeIndex != -1 ? _complexeRecipes[_lastRecipeIndex].Value : null;
		public List<CompatibleIngredient> GetSelectedIngredients() =>
				_ingredientSelectors
				.Where(s => s?.AssignedIngredient != null && !s.IsMissingIngredient)
				.Select(s => s.AssignedIngredient).ToList();

		#region Copies of Base Methods for Debugging 
#if DEBUG
		private void DebugShow()
		{
			this.SetPrivateField<Panel, bool>("m_showWanted", true);
			Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking UIElement_Show()");
			UIElement_Show();
			Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking Panel_Show()");
			Panel_Show();
			Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking MenuPanel_Show()");
			MenuPanel_Show();
			Logger.LogDebug($"CustomCraftingMenu::Show(): Invoking CustomShow()");
			CustomShow();

			resetFreeRecipeLastIngredients();
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
			_allRecipes = RecipeManager.Instance.GetRecipes(GetRecipeCraftingType(), base.LocalCharacter);

			Logger.LogDebug($"CustomShow: 12");
			//m_refreshComplexeRecipeRequired = true;
			_refreshComplexeRecipeRequired = true;

			Logger.LogDebug($"CustomShow: 13");
			//RefreshAutoRecipe();
			refreshAutoRecipe();

			Logger.LogDebug($"CustomShow: 14");
			OnRecipeSelected(-1, _forceRefresh: true);
		}
		private bool TryOnRecipeSelectedOverride(int _index, bool _forceRefresh = false)
		{
			try
			{
				OnRecipeSelectedOverride(_index, _forceRefresh);
				return true;
			}
			catch (Exception ex)
			{
				Logger.LogException($"{nameof(CustomCraftingMenu)}::{nameof(TryOnRecipeSelectedOverride)}(): Exception Invoking {nameof(OnRecipeSelectedOverride)}().", ex);
			}
			return false;
		}
		private void OnRecipeSelectedOverride(int _index, bool _forceRefresh = false)
		{
			Logger.LogDebug($"OnRecipeSelectedOverride: 1");
			if (_lastRecipeIndex == _index && !_forceRefresh)
				return;
			Logger.LogDebug($"OnRecipeSelectedOverride: 2");
			if (IsCraftingInProgress)
				cancelCrafting();

			Logger.LogDebug($"OnRecipeSelectedOverride: 3");
			if (_lastRecipeIndex != -1)
				_recipeDisplays[_lastRecipeIndex].SetHighlight(_highlight: false);
			else
				_freeRecipeDisplay.SetHighlight(_highlight: false);

			Logger.LogDebug($"OnRecipeSelectedOverride: 4");
			for (int i = 0; i < _ingredientSelectors.Length; i++)
				_ingredientSelectors[i].Free(_resetUseCount: true);

			_lastRecipeIndex = _index;
			Logger.LogDebug($"OnRecipeSelectedOverride: 5");
			_itemDetailPanel.Show(_lastRecipeIndex != -1);
			_freeRecipeDescriptionPanel.gameObject.SetActive(_lastRecipeIndex == -1);
			Logger.LogDebug($"OnRecipeSelectedOverride: 6");
			if (_index != -1)
			{
				resetFreeRecipeLastIngredients();
				Logger.LogDebug($"OnRecipeSelectedOverride: 7 | _index: {_index}, _complexeRecipes.Count: {_complexeRecipes.Count}");
				Logger.LogDebug($"OnRecipeSelectedOverride: 7.5 |_complexeRecipes[_index].Value.Results[0]: {_complexeRecipes[_index].Value.Results[0].ItemID}");
				_recipeResultDisplay.SetRecipeResult(_complexeRecipes[_index].Value.Results[0]);
				Logger.LogDebug($"OnRecipeSelectedOverride: 8");
				RefreshItemDetailDisplay(_recipeResultDisplay);
				Logger.LogDebug($"OnRecipeSelectedOverride: 9");
				setCraftButtonEnable(_recipeDisplays[_index].IsRecipeIngredientsComplete);
				Logger.LogDebug($"OnRecipeSelectedOverride: 10");
				for (int num = _ingredientSelectors.Length - 1; num >= 0; num--)
				{
					Logger.LogDebug($"OnRecipeSelectedOverride: F01");
					if (num < _complexeRecipes[_index].Value.Ingredients.Length)
					{
						Logger.LogDebug($"OnRecipeSelectedOverride: F02");
						if (num < _ingredientSelectors.Length - 1)
						{
							if (_ingredientSelectors[num + 1].Button.interactable)
							{
								Logger.LogDebug($"OnRecipeSelectedOverride: F03A");
								((Selectable)(object)_ingredientSelectors[num].Button).SetRightNav((Selectable)(object)_ingredientSelectors[num + 1].Button);
							}
							else
							{
								((Selectable)(object)_ingredientSelectors[num].Button).SetRightNav(null);
								Logger.LogDebug($"OnRecipeSelectedOverride: F03B");
							}
						}
						_ingredientSelectors[num].Button.interactable = (true);
						Logger.LogDebug($"OnRecipeSelectedOverride: F04");
						int num2 = _recipeDisplays[_index].BestIngredientsPerStep[num];
						Logger.LogDebug($"OnRecipeSelectedOverride: F05");
						_ingredientSelectors[num].Set(_complexeRecipes[_index].Value.Ingredients[num], (num2 != -1) ? _availableIngredients[num2] : null);
						Logger.LogDebug($"OnRecipeSelectedOverride: F06");
					}
					else
					{
						_ingredientSelectors[num].Clear();
						Logger.LogDebug($"OnRecipeSelectedOverride: G01");
						_ingredientSelectors[num].Button.interactable = (false);
						Logger.LogDebug($"OnRecipeSelectedOverride: G02");
					}
				}
				Logger.LogDebug($"OnRecipeSelectedOverride: 11");
				//_recipeResultDisplay.SetRecipeResult(_complexeRecipes[_index].Value.Results[0]);
				Logger.LogDebug($"OnRecipeSelectedOverride: 12");
				//RefreshItemDetailDisplay(_recipeResultDisplay);
				Logger.LogDebug($"OnRecipeSelectedOverride: 13");
				_recipeDisplays[_index].SetHighlight(_highlight: true);
				Logger.LogDebug($"OnRecipeSelectedOverride: 14");
				return;
			}
			_freeRecipeDisplay.SetHighlight(_highlight: true);
			bool flag = false;
			_recipeResultDisplay.Clear();
			for (int j = 0; j < _ingredientSelectors.Length; j++)
			{
				_ingredientSelectors[j].Button.interactable = true;
				if (_lastFreeRecipeIngredientIDs[j] != -1)
				{
					int key = _lastFreeRecipeIngredientIDs[j];
					if (_availableIngredients.TryGetValue(key, out var _outValue) && _outValue.AvailableQty > 0)
					{
						_ingredientSelectors[j].Set(null, _outValue);
					}
					else
					{
						_ingredientSelectors[j].Set(null, null);
					}
					_lastFreeRecipeIngredientIDs[j] = -1;
				}
				else
				{
					_ingredientSelectors[j].Set(null, null);
				}
				if (j < _ingredientSelectors.Length - 1)
				{
					((Selectable)(object)_ingredientSelectors[j].Button).SetRightNav((Selectable)(object)_ingredientSelectors[j + 1].Button);
				}
				flag |= !_ingredientSelectors[j].IsMissingIngredient;
			}
			setCraftButtonEnable(flag);
			refreshFreeRecipeResult();
			for (int k = 0; k < _ingredientSelectors.Length; k++)
			{
				if (_ingredientSelectors[k].IsMissingIngredient)
				{
					_lastFreeRecipeIngredientIDs[k] = -1;
				}
			}
		}
#endif
		#endregion
	}
}
