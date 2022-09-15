using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Modules.Crafting.Patches;
using ModifAmorphic.Outward.Modules.Crafting.Services;
using ModifAmorphic.Outward.Patches;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CustomCraftingModule : IModifModule
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly CraftingMenuUIService _menuUiService;
        private readonly RecipeDisplayService _recipeDisplayService;
        internal RecipeDisplayService RecipeDisplayService => _recipeDisplayService;
        private readonly CustomRecipeService _customRecipeService;
        internal CustomRecipeService CustomRecipeService => _customRecipeService;
        private readonly CustomCraftingService _craftingService;
        internal CustomCraftingService CustomCraftingService => _craftingService;


        private readonly List<CraftingMenuMetadata> _registeredMenus =
            new List<CraftingMenuMetadata>();

        private readonly ConcurrentDictionary<int, List<CraftingMenuMetadata>> _characterMenus =
            new ConcurrentDictionary<int, List<CraftingMenuMetadata>>();

        private readonly ConcurrentDictionary<Type, CraftingMenuMetadata> _craftingMenus =
           new ConcurrentDictionary<Type, CraftingMenuMetadata>();

        private readonly ConcurrentDictionary<Type, Dictionary<string, RecipeMetadata>> _customRecipes =
           new ConcurrentDictionary<Type, Dictionary<string, RecipeMetadata>>();

        private readonly ConcurrentDictionary<Type, Recipe.CraftingType> _craftingStationTypes =
            new ConcurrentDictionary<Type, Recipe.CraftingType>();

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(CharacterUIPatches),
            typeof(CraftingMenuPatches),
            typeof(CompatibleIngredientPatches),
            typeof(LocalizationManagerPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(CharacterUIPatches),
            typeof(CraftingMenuPatches),
            typeof(LocalizationManagerPatches)
        };

        public HashSet<Type> DepsWithMultiLogger => new HashSet<Type>() {
            typeof(CharacterUIPatches),
            typeof(CraftingMenuPatches),
            typeof(CompatibleIngredientPatches),
            typeof(LocalizationManagerPatches)
        };

        public readonly CraftingMenuEvents CraftingMenuEvents;
        internal CustomCraftingModule(CraftingMenuUIService menuUIService, RecipeDisplayService recipeDisplayService, CustomRecipeService customRecipeService, CustomCraftingService craftingService, CraftingMenuEvents craftingMenuEvents, Func<IModifLogger> loggerFactory)
        {
            (_menuUiService, _recipeDisplayService, _customRecipeService, _craftingService, CraftingMenuEvents, _loggerFactory) =
                (menuUIService, recipeDisplayService, customRecipeService, craftingService, craftingMenuEvents, loggerFactory);
            CharacterUIPatches.AwakeBefore += CharacterUIPatches_AwakeBefore;
            CharacterUIPatches.RegisterMenuAfter += CharacterUIPatches_RegisterMenuAfter;
            CraftingMenuPatches.AwakeInitAfter += CraftingMenuPatches_AwakeInitAfter;

            recipeDisplayService.MenuHiding += (menu) => CraftingMenuEvents.InvokeMenuHiding(menu);
        }

        private void CraftingMenuPatches_AwakeInitAfter(CraftingMenu craftingMenu)
        {
            if (craftingMenu is CustomCraftingMenu)
            {
                //Needed to avoid infinite recursion
                return;
            }

            AddCustomCraftingMenus(craftingMenu);
        }
        //internal void RaiseMenuLoaded(CustomCraftingMenu menu) => MenuLoaded?.Invoke(menu);

        private void CharacterUIPatches_AwakeBefore(CharacterUI characterUI)
        {
            AddCraftingTabAndFooter(characterUI);
        }

        private void CharacterUIPatches_RegisterMenuAfter(CharacterUI characterUI, CustomCraftingMenu craftingMenu)
        {
            var menuType = craftingMenu.GetType();
            var playerID = characterUI.RewiredID;
            _craftingMenus.TryGetValue(menuType, out var metaParent);

            var menuTabs = characterUI.GetPrivateField<CharacterUI, MenuTab[]>("m_menuTabs");

            var meta = metaParent.Clone();
            meta.MenuType = menuType;
            meta.MenuPanel = craftingMenu;
            meta.MenuDisplay = craftingMenu.gameObject;
            meta.MenuTab = menuTabs.First(t => t.TabName == meta.TabName).Tab.gameObject;
            meta.MenuFooter = _menuUiService.GetFooter(characterUI, meta.FooterName);
            var metas = _characterMenus.GetOrAdd(playerID, new List<CraftingMenuMetadata>());
            metas.Add(meta);

            CraftingMenuEvents?.InvokeMenuLoading(craftingMenu);
        }

        /// <summary>
        /// Registers a new crafting menu to be added when the base CharacterUI menus are normally added. This should be called sometime during a plugin's Awake to insure it gets added.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="CustomCraftingMenu"/> to register.</typeparam>
        /// <param name="craftingMenu"></param>
        /// <param name="menuDisplayName"></param>
        public Recipe.CraftingType RegisterCraftingMenu<T>(string menuDisplayName, MenuIcons menuIcons = null) where T : CustomCraftingMenu
        {
            if (_craftingMenus.ContainsKey(typeof(T)))
                throw new ArgumentException($"{nameof(CustomCraftingMenu)} of type {typeof(T)} already exists. " +
                    $"Only one instance of a type derived from {nameof(CustomCraftingMenu)} can be added.", nameof(T));

            if (menuIcons != null)
                menuIcons.TrySetIconNames(menuDisplayName);

            //var orderNo = _craftingMenus.Count;
            _craftingMenus.TryAdd(typeof(T),
                new CraftingMenuMetadata()
                {
                    MenuType = typeof(T),
                    TabButtonName = "btn" + typeof(T).Name,
                    TabName = "PlayerMenu_Tab_" + typeof(T).Name,
                    TabDisplayName = menuDisplayName,
                    MenuName = typeof(T).Name,
                    MenuIcons = menuIcons,
                    FooterName = typeof(T).Name + "Footer",
                    //TabOrderNo = orderNo
                });

            _registeredMenus.Add(_craftingMenus[typeof(T)]);

            _craftingStationTypes.TryAdd(typeof(T),
                (Recipe.CraftingType)_menuUiService.AddIngredientTag());

            TryAddRecipes();

            return _craftingStationTypes[typeof(T)];

        }
        /// <summary>
        /// Registers a new recipe to be added once the <see cref="Recipe.CraftingType" /> is known for the 
        /// <see cref="CustomCraftingMenu"/> type of <typeparamref name="T"/>. If the <see cref="Recipe.CraftingType" />
        /// is already known, then the recipe will be added immedately.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="CustomCraftingMenu"/> to register the new <paramref name="recipe"/> for.</typeparam>
        /// <param name="recipe">The new <see cref="Recipe"/></param>
        public void RegisterRecipe<T>(Recipe recipe) where T : CustomCraftingMenu
        {
            ValidateRecipe(recipe);
            Logger.LogTrace($"{nameof(CustomCraftingModule)}:{nameof(RegisterRecipe)}:: Recipe {recipe.name} is valid.  Registering recipe.");
            var recipes = _customRecipes.GetOrAdd(typeof(T), new Dictionary<string, RecipeMetadata>());

            if (recipes.ContainsKey(recipe.UID.ToString()))
                throw new ArgumentException($"A Recipe with UID '{recipe.UID}' is already registered for crafting station type {typeof(T).Name}."
                    , nameof(recipe));

            recipes.Add(recipe.UID.ToString(),
                new RecipeMetadata()
                {
                    CraftingStationType = typeof(T),
                    Recipe = recipe
                });
            TryAddRecipes();
        }
        public List<Recipe> GetRegisteredRecipes<T>() => _customRecipes.TryGetValue(typeof(T), out var recipes) ? recipes.Values.Select(r => r.Recipe).ToList() : new List<Recipe>();
        public void RegisterCustomCrafter<T>(ICustomCrafter crafter) where T : CustomCraftingMenu => _craftingService.AddOrUpdateCrafter<T>(crafter);
        public void RegisterMenuIngredientFilters<T>(MenuIngredientFilters filter) where T : CustomCraftingMenu
            => _craftingService.AddOrUpdateIngredientFilter<T>(filter);
        public void UnregisterMenuIngredientFilters<T>() where T : CustomCraftingMenu
            => _craftingService.TryRemoveIngredientFilter<T>();
        public bool TryGetRegisteredIngredientFilters<T>(out MenuIngredientFilters filter) where T : CustomCraftingMenu
            => _craftingService.TryGetIngredientFilter<T>(out filter);
        public void RegisterRecipeVisibiltyController<T>(IRecipeVisibiltyController visibiltyController) where T : CustomCraftingMenu
            => _recipeDisplayService.AddOrUpdateRecipeVisibiltyController<T>(visibiltyController);
        public void UnregisterRecipeVisibiltyController<T>() where T : CustomCraftingMenu
            => _recipeDisplayService.TryRemoveRecipeVisibiltyController<T>();
        public void RegisterCompatibleIngredientMatcher<T>(ICompatibleIngredientMatcher matcher) where T : CustomCraftingMenu
            => _craftingService.AddOrUpdateCompatibleIngredientMatcher<T>(matcher);
        public void RegisterConsumedItemSelector<T>(IConsumedItemSelector itemSelector) where T : CustomCraftingMenu
            => _craftingService.AddOrUpdateConsumedItemSelector<T>(itemSelector);
        public void UnregisterConsumedItemSelector<T>() where T : CustomCraftingMenu
           => _craftingService.TryRemoveConsumedItemSelector<T>();
        public void RegisterRecipeSelectorDisplayConfig<T>(RecipeSelectorDisplayConfig config) where T : CustomCraftingMenu
            => _recipeDisplayService.AddUpdateDisplayConfig<T>(config);
        public bool TryGetRegisteredRecipeDisplayConfig<T>(out RecipeSelectorDisplayConfig config) where T : CustomCraftingMenu
            => _recipeDisplayService.TryGetDisplayConfig<T>(out config);
        public void RegisterStaticIngredients<T>(IEnumerable<StaticIngredient> ingredients) where T : CustomCraftingMenu
            => _recipeDisplayService.AddOrUpdateStaticIngredients<T>(ingredients);
        public void UnregisterStaticIngredients<T>() where T : CustomCraftingMenu
            => _recipeDisplayService.TryRemoveStaticIngredients<T>();


        public void EnableCraftingMenu<T>() where T : CustomCraftingMenu
        {
            if (!_craftingMenus.TryGetValue(typeof(T), out var menuData))
            {
                throw new ArgumentException($"CustomCraftingMenu {typeof(T)} is not a registered menu type. Menus " +
                    $"must be registered using RegisterCraftingMenu<T> and loaded before they can be enabled or disabled.", nameof(T));
            }

            var characters = SplitScreenManager.Instance.LocalPlayers.Select(p => p.AssignedCharacter);
            foreach (var c in characters)
            {
                var playerID = c.OwnerPlayerSys.PlayerID;
                if (_characterMenus.TryGetValue(playerID, out var menus))
                {
                    var meta = menus.FirstOrDefault(m => m.MenuType == typeof(T));
                    _menuUiService.EnableMenuTab(c.CharacterUI, meta);
                }
            }
        }
        public void DisableCraftingMenu<T>() where T : CustomCraftingMenu
        {
            if (!_craftingMenus.TryGetValue(typeof(T), out var menuData))
            {
                throw new ArgumentException($"CustomCraftingMenu {typeof(T)} is not a registered menu type. Menus " +
                    $"must be registered using RegisterCraftingMenu<T> and loaded before they can be enabled or disabled.", nameof(T));
            }
            var characters = SplitScreenManager.Instance.LocalPlayers.Select(p => p.AssignedCharacter);
            foreach (var c in characters)
            {
                var playerID = c.OwnerPlayerSys.PlayerID;
                if (_characterMenus.TryGetValue(playerID, out var menus))
                {
                    var meta = menus.FirstOrDefault(m => m.MenuType == typeof(T));
                    _menuUiService.DisableMenuTab(c.CharacterUI, meta);
                }
            }
        }


        private void ValidateRecipe(Recipe recipe)
        {
            if (recipe.IsFullySetup)
                return;

            var exPrefix = $"Recipe '{recipe.RecipeID} - {recipe.Name}' IsFullySetup is false.";
            if (recipe.Results == null || recipe.Results.Length == 0 || recipe.Results[0] == null)
                throw new ArgumentException($"{exPrefix} The recipe's Results are not configured properly. Either the Results are empty or null.", "recipe");

            if (recipe.Results[0].RefItem == null)
                throw new ArgumentException($"{exPrefix} Recipe's Results[0].RefItem is null. Ensure ItemID '{recipe.Results[0].ItemID}' of the result has a valid Prefab.", "recipe");

            if (recipe.Ingredients == null || recipe.Ingredients.Length == 0)
                throw new ArgumentException($"{exPrefix} Recipe's Ingredients are null or empty. Recipe must have ingredients set.", "recipe");

            for (int i = 0; i < recipe.Ingredients.Length; i++)
            {
                if ((recipe.Ingredients[i].ActionType == RecipeIngredient.ActionTypes.AddSpecificIngredient && recipe.Ingredients[i].AddedIngredient == null))
                {
                    throw new ArgumentException($"{exPrefix} Recipe ingredient Ingredients[{i}] is an AddSpecificIngredient ActionType, but has no AddedIngredient set. " +
                        $"An AddSpecificIngredient ingredient must have a non null AddedIngredient.", "recipe");
                }
                if ((recipe.Ingredients[i].ActionType == RecipeIngredient.ActionTypes.AddGenericIngredient && !recipe.Ingredients[i].AddedIngredientType.IsSet))
                {
                    throw new ArgumentException($"{exPrefix} Recipe ingredient Ingredients[{i}] is an AddGenericIngredient ActionType, but it's AddedIngredientType.IsSet property returned false. " +
                        $"Ensure the Tag is configured properly for this AddGenericIngredient ingredient type.", "recipe");
                }
            }
        }

        private void TryAddRecipes()
        {
            //get only recipes for crafting stations where the Custom CraftingType is known - basically, the addition of the custom crafting menu is
            //far enough along to where the CraftingType has been determined.
            var stations = _customRecipes.Where(rt => _craftingStationTypes.ContainsKey(rt.Key));
            foreach (var stationRecipes in stations)
            {
                var recipes = stationRecipes.Value
                    .Where(kvp => kvp.Value.CustomCraftingType == default)
                    .Select(kvp =>
                    {
                        //Set this value to the type to mark it as added.
                        kvp.Value.CustomCraftingType = _craftingStationTypes[stationRecipes.Key];
                        //set the actual recipe value as well.
                        kvp.Value.Recipe.SetCraftingType(_craftingStationTypes[stationRecipes.Key]);
                        return kvp.Value.Recipe;
                    }).ToList();
                Logger.LogDebug($"{nameof(CustomCraftingModule)}:{nameof(TryAddRecipes)}:: Registering {recipes?.Count()} recipes for crafting station type {stationRecipes.Key}.");
                _customRecipeService.AddRecipes(recipes);
            }
        }
        private void AddCraftingTabAndFooter(CharacterUI characterUI)
        {
            var menuTypes = characterUI.GetPrivateField<CharacterUI, Type[]>("MenuTypes");

            var screenNo = menuTypes.Length;
            Array.Resize(ref menuTypes, menuTypes.Length + _craftingMenus.Count);

            //Crafting menus get inserted right after the base crafting menu, so add the last added menu first.
            //this way the order the menus were registered in is preserved.
            for (int i = _registeredMenus.Count - 1; i >= 0; i--)
            {
                var menu = _registeredMenus[i];

                menu.MenuScreenNo = screenNo++;

                menuTypes[menu.MenuScreenNo] = menu.MenuType;
                _menuUiService.AddMenuTab(characterUI, menu.TabName, menu.TabDisplayName, menu.TabButtonName, menu.MenuScreenNo, menu.OrderAfterTab, menu.MenuIcons);
                _menuUiService.AddFooter(characterUI, menu.MenuScreenNo, menu.FooterName);
            }

            characterUI.SetPrivateField("MenuTypes", menuTypes);

            var m_menus = characterUI.GetPrivateField<CharacterUI, MenuPanel[]>("m_menus");
            if (m_menus != null && m_menus.Length < menuTypes.Length)
                Array.Resize(ref m_menus, menuTypes.Length);
            else
                m_menus = new MenuPanel[menuTypes.Length];

            characterUI.SetPrivateField("m_menus", m_menus);
        }
        private void AddCustomCraftingMenus(CraftingMenu baseCraftingMenu)
        {

            foreach (var kvp in _craftingMenus)
            {
                (Type menuType, CraftingMenuMetadata meta) = (kvp.Key, kvp.Value);

                var customMenuGo = _menuUiService.AddCustomMenu(baseCraftingMenu, meta);

                //backfill the Crafting Station type on the newly created CustomCraftingMenu instance if it's not a permanent crafting station
                var customMenu = (CustomCraftingMenu)customMenuGo.GetComponent(menuType);
                if (customMenu.PermanentCraftingStationType == null)
                {
                    customMenu.CustomCraftingType = _craftingStationTypes[menuType];
                    Logger.LogDebug($"{nameof(CustomCraftingModule)}::{nameof(AddCustomCraftingMenus)}(): " +
                        $"Set {nameof(CustomCraftingMenu.CustomCraftingType)} to {_craftingStationTypes[menuType]} for " +
                        $"type {meta.MenuType }");
                }
                // Avoids key not found exceptions if there is a custom crafting station with no actual recipes.
                if (customMenu.IsCustomCraftingStation())
                    _customRecipeService.AddOrGetCraftingStationRecipes(customMenu.GetRecipeCraftingType());

            }
        }
    }
}
